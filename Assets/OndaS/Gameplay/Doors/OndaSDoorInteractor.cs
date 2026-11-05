using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using OndaS.Gameplay.UI;
using OndaS.Gameplay.Items;
using OndaS.Gameplay.Earthquake;
using OndaS.Gameplay.Electrical;
using OndaS.Gameplay.Gas;

namespace OndaS.Gameplay
{
    public sealed class OndaSDoorInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private Vector3 heldItemPosition = new Vector3(0.3f, -0.25f, 0.6f);
        [SerializeField, Min(0.05f)] private float heldItemSize = 0.22f;
        [Header("Linterna en mano")]
        [SerializeField] private Vector3 flashlightPosition = new Vector3(0.26f, -0.245f, 0.68f);
        [SerializeField] private Vector3 flashlightRotation = new Vector3(0f, -90f, 0f);
        [SerializeField, Min(0.05f)] private float flashlightSize = 0.32f;
        [Header("Agua en mano")]
        [SerializeField] private Vector3 waterPosition = new Vector3(0.26f, -0.32f, 0.68f);
        [SerializeField] private Vector3 waterRotation = Vector3.zero;
        [SerializeField, Min(0.05f)] private float waterSize = 0.30f;
        [SerializeField, Min(0.2f)] private float drinkDuration = 1.3f;
        private GameObject promptCanvas, heldVisual;
        private bool holdingFlashlight, holdingWater;
        private OndaSCollectible drinkingItem;
        private int drinkingSlot;
        private float drinkElapsed, drinkPose;
        private TextMeshProUGUI promptText;
        private OndaSInventory inventory;
        private OndaS.Visual.OndaSLookDevWalkthrough playerMovement;
        private OndaSFirstPersonHands firstPersonHands;
        private OndaSInventoryHud inventoryHud;
        private OndaSBackpackRadial radial;
        private OndaSBackpack focusedBag;
        private float messageUntil;
        private string message;
        private OndaSShelterInteraction shelter;
        private OndaSFuseBox inspectedFuseBox;
        private OndaSGasValve inspectedGasValve;
        private void Awake()
        {
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            inventory = GetComponent<OndaSInventory>();
            playerMovement = GetComponent<OndaS.Visual.OndaSLookDevWalkthrough>();
            firstPersonHands = viewCamera != null ? viewCamera.GetComponentInChildren<OndaSFirstPersonHands>(true) : null;
            shelter = new OndaSShelterInteraction(transform, viewCamera, playerMovement);
            if (inventory == null) inventory = gameObject.AddComponent<OndaSInventory>();
            CreatePrompt();
            var hud = new GameObject("Inventory UI", typeof(RectTransform)); inventoryHud = hud.AddComponent<OndaSInventoryHud>(); inventoryHud.Initialize(inventory);
            var menu = new GameObject("Backpack Radial UI", typeof(RectTransform)); radial = menu.AddComponent<OndaSBackpackRadial>(); radial.Initialize();
            inventory.Changed += RefreshHeld;
        }
        private void Update()
        {
            if (heldVisual != null && playerMovement != null)
                heldVisual.SetActive(playerMovement.enabled);
            if (holdingFlashlight && heldVisual != null) UpdateFlashlightPose();
            if (holdingWater && heldVisual != null) UpdateWaterPose();
            if (drinkingItem != null)
            {
                UpdateDrinking();
                return;
            }
            if (inspectedGasValve != null)
            {
                if (!inspectedGasValve.IsInspecting) { inspectedGasValve = null; ShowPrompt(null); return; }
                inspectedGasValve.HandleInspectionInput();
                ShowPrompt(inspectedGasValve.IsInspecting ? (inspectedGasValve.IsClosed ? "CLIC: ABRIR VÁLVULA    [E] SALIR" : "CLIC: CERRAR VÁLVULA    [E] SALIR") : null);
                return;
            }
            if (inspectedFuseBox != null)
            {
                if (!inspectedFuseBox.IsInspecting) { inspectedFuseBox = null; ShowPrompt(null); return; }
                inspectedFuseBox.HandleInspectionInput();
                ShowPrompt(inspectedFuseBox.IsInspecting ? "CLIC: MOVER INTERRUPTOR    [E] CERRAR" : null);
                return;
            }
            if (viewCamera == null || Cursor.lockState != CursorLockMode.Locked)
            { CancelRadial(); ShowPrompt(null); return; }
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            bool hitSomething = Physics.Raycast(ray, out var hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore);
            var bag = hitSomething ? hit.collider.GetComponentInParent<OndaSBackpack>() : null;
            if (radial.Active)
            {
                if (keyboard == null || focusedBag == null || bag != focusedBag || keyboard.escapeKey.wasPressedThisFrame)
                { CancelRadial(); ShowPrompt(null); return; }
                if (mouse != null) radial.Move(mouse.delta.ReadValue());
                if (keyboard.eKey.wasReleasedThisFrame)
                {
                    int choice = radial.Selected;
                    var target = focusedBag;
                    CancelRadial();
                    if (choice == 4)
                    {
                        if (inventory.EquipBackpack(target)) Notify("Mochila equipada");
                        else Notify("Ya llevas una mochila");
                    }
                    else if (choice >= 0)
                    {
                        inventory.Transfer(target, choice, inventoryHud.Selected, out var feedback);
                        Notify(feedback);
                    }
                    ShowPrompt(null);
                }
                return;
            }
            // Slot selection remains available while crouching or sheltered, even
            // when a nearby interaction supplies the current prompt.
            if (keyboard != null)
            {
                int old = inventoryHud.Selected;
                if (keyboard.digit1Key.wasPressedThisFrame) inventoryHud.Select(0);
                if (keyboard.digit2Key.wasPressedThisFrame) inventoryHud.Select(1);
                if (keyboard.digit3Key.wasPressedThisFrame) inventoryHud.Select(2);
                if (old != inventoryHud.Selected) RefreshHeld();
            }
            if (shelter != null && shelter.Update(keyboard, out var shelterPrompt))
            { ShowPrompt(shelterPrompt); return; }
            if (holdingWater && playerMovement != null && playerMovement.enabled && mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (playerMovement.Stamina < playerMovement.MaxStamina)
                {
                    drinkingSlot = inventoryHud.Selected;
                    drinkingItem = inventory.GetItem(drinkingSlot);
                    drinkElapsed = 0f;
                }
                else Notify("La estamina ya está llena");
                ShowPrompt(null);
                return;
            }
            var fuseBox = hitSomething ? hit.collider.GetComponentInParent<OndaSFuseBox>() : null;
            if (fuseBox != null)
            {
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame && fuseBox.BeginInspection(viewCamera, GetComponent<OndaS.Visual.OndaSLookDevWalkthrough>()))
                    inspectedFuseBox = fuseBox;
                ShowPrompt("[E] Para abrir");
                return;
            }
            var gasValve = hitSomething ? hit.collider.GetComponentInParent<OndaSGasValve>() : null;
            if (gasValve != null)
            {
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame && gasValve.BeginInspection(viewCamera, GetComponent<OndaS.Visual.OndaSLookDevWalkthrough>()))
                    inspectedGasValve = gasValve;
                ShowPrompt("[E] Inspeccionar válvula");
                return;
            }
            if (keyboard != null)
            {
                if (keyboard.qKey.wasPressedThisFrame) { DropSelected(); ShowPrompt(null); return; }
            }
            if (bag != null && !bag.IsEquipped)
            {
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                {
                    if (!bag.WasEquipped)
                    {
                        if (inventory.EquipBackpack(bag)) Notify("Mochila equipada"); else Notify("Ya llevas una mochila");
                    }
                    else { focusedBag = bag; radial.Open(bag, viewCamera); ShowPrompt(null); return; }
                }
                ShowPrompt(bag.WasEquipped ? "MOCHILA\n[E] Mantener para abrir" : "MOCHILA\n[E] Recoger"); return;
            }
            var collectible = hitSomething ? hit.collider.GetComponentInParent<OndaSCollectible>() : null;
            if (collectible != null && collectible.Item != null)
            {
                if (!inventory.HasFreeSlot) { ShowPrompt("Espacios rápidos llenos"); return; }
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame && inventory.TryAdd(collectible))
                { Notify("Recogiste " + collectible.Item.DisplayName); ShowPrompt(null); return; }
                ShowPrompt("[E] Recoger " + collectible.Item.DisplayName); return;
            }
            var door = hitSomething ? hit.collider.GetComponentInParent<OndaSDoor>() : null;
            if (door != null && keyboard != null && keyboard.eKey.wasPressedThisFrame) door.Toggle();
            ShowPrompt(door != null && door.CanInteract ? (door.IsOpen ? "[E] Cerrar puerta" : "[E] Abrir puerta") : null);
        }
        private void CancelRadial() { if (radial != null && radial.Active) radial.Close(); focusedBag = null; }
        private void OnDisable() { CancelDrinking(); if (inspectedFuseBox != null) inspectedFuseBox.InterruptInspection(); if (inspectedGasValve != null) inspectedGasValve.InterruptInspection(); inspectedFuseBox = null; inspectedGasValve = null; CancelRadial(); if (playerMovement != null) playerMovement.SetFlashlightEquipped(false); if (firstPersonHands != null) firstPersonHands.SetHoldingItem(false); if (heldVisual != null) heldVisual.SetActive(false); if (promptCanvas != null) promptCanvas.SetActive(false); if (inventoryHud != null) inventoryHud.gameObject.SetActive(false); }
        private void OnEnable() { if (inventoryHud != null) { inventoryHud.gameObject.SetActive(true); RefreshHeld(); } }
        private void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= RefreshHeld;
            if (heldVisual != null) Destroy(heldVisual);
            if (promptCanvas != null) Destroy(promptCanvas);
            if (inventoryHud != null) Destroy(inventoryHud.gameObject);
            if (radial != null) Destroy(radial.gameObject);
        }
        private void ShowPrompt(string value)
        {
            if (Time.unscaledTime < messageUntil) value = message;
            if (promptCanvas == null) return;
            promptCanvas.SetActive(!string.IsNullOrEmpty(value));
            if (!string.IsNullOrEmpty(value)) promptText.text = value;
        }
        private void DropSelected()
        {
            int slot = inventoryHud.Selected;
            var item = inventory.GetItem(slot);
            var backpack = slot == OndaSInventory.BackpackSlot ? inventory.Backpack : null;
            if (item == null && backpack == null) { Notify("Espacio vacío"); return; }
            if (!TryDropPosition(out Vector3 point)) { Notify("No hay suelo para soltar"); return; }
            if (backpack != null) { inventory.DetachBackpack(); backpack.Drop(point); Notify("Mochila en el suelo"); }
            else { inventory.Remove(slot); item.Drop(point); Notify("Soltaste " + item.Item.DisplayName); }
        }
        private bool TryDropPosition(out Vector3 point)
        {
            point = default;
            Vector3 forward = Vector3.ProjectOnPlane(viewCamera.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
            Vector3 origin = viewCamera.transform.position; float distance = 1.25f;
            if (Physics.Raycast(origin, forward, out var wall, distance, interactionLayers, QueryTriggerInteraction.Ignore)) distance = Mathf.Max(0, wall.distance - 0.3f);
            if (distance < 0.45f) return false;
            var hits = Physics.RaycastAll(origin + forward * distance, Vector3.down, 3f, interactionLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.GetComponentInParent<OndaSCollectible>() != null || hit.collider.GetComponentInParent<OndaSBackpack>() != null || hit.normal.y < 0.6f || hit.distance >= nearest) continue;
                nearest = hit.distance; point = hit.point + Vector3.up * 0.02f;
            }
            return !float.IsPositiveInfinity(nearest);
        }
        private void Notify(string value) { message = value; messageUntil = Time.unscaledTime + 1.5f; }
        private void RefreshHeld()
        {
            if (heldVisual != null) Destroy(heldVisual);
            var item = inventoryHud != null ? inventory.GetItem(inventoryHud.Selected) : null;
            holdingFlashlight = item != null && item.Item != null && item.Item.IsFlashlight;
            holdingWater = item != null && item.Item != null && item.Item.RestoresStamina;
            if (playerMovement != null) playerMovement.SetFlashlightEquipped(holdingFlashlight);
            if (firstPersonHands != null) firstPersonHands.SetHoldingItem(holdingFlashlight || holdingWater);
            if (item == null || item.Item == null || item.Item.WorldPrefab == null || viewCamera == null) return;
            heldVisual = Instantiate(item.Item.WorldPrefab, viewCamera.transform);
            heldVisual.name = "Held item visual (non-interactive)";
            foreach (var behaviour in heldVisual.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
            foreach (var collider in heldVisual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (var body in heldVisual.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic = true; body.detectCollisions = false; }
            heldVisual.transform.localPosition = heldItemPosition; heldVisual.transform.localRotation = Quaternion.identity;
            var renderers = heldVisual.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                if (largest > 0.001f) heldVisual.transform.localScale *= heldItemSize / largest;
            }
            if (holdingFlashlight)
                UpdateFlashlightPose();
            else if (holdingWater)
                UpdateWaterPose();
        }
        private void UpdateFlashlightPose()
        {
            // The prefab is modeled along +X and is 0.24 units long.
            heldVisual.transform.localPosition = flashlightPosition;
            heldVisual.transform.localRotation = Quaternion.Euler(flashlightRotation);
            heldVisual.transform.localScale = Vector3.one * (flashlightSize / 0.24f);
        }
        private void UpdateWaterPose()
        {
            // The water prefab is modeled upright and is 0.28 units tall.
            heldVisual.transform.localPosition = waterPosition + new Vector3(-0.10f, 0.08f, -0.16f) * drinkPose;
            heldVisual.transform.localRotation = Quaternion.Euler(waterRotation) * Quaternion.Euler(-55f * drinkPose, 0f, 25f * drinkPose);
            heldVisual.transform.localScale = Vector3.one * (waterSize / 0.28f);
        }
        private void UpdateDrinking()
        {
            if (playerMovement == null || !playerMovement.isActiveAndEnabled || viewCamera == null || Cursor.lockState != CursorLockMode.Locked || inventory.GetItem(drinkingSlot) != drinkingItem)
            {
                CancelDrinking();
                return;
            }
            drinkElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(drinkElapsed / Mathf.Max(0.2f, drinkDuration));
            float lift = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.3f));
            float lower = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - progress) / 0.3f));
            drinkPose = Mathf.Min(lift, lower);
            if (firstPersonHands != null) firstPersonHands.SetDrinkingPose(drinkPose);
            if (heldVisual != null) UpdateWaterPose();
            if (progress < 1f) return;
            var consumed = inventory.Remove(drinkingSlot);
            CancelDrinking();
            if (consumed == null) return;
            Destroy(consumed.gameObject);
            playerMovement.RestoreStamina();
            Notify("Bebiste agua: estamina recuperada");
        }
        private void CancelDrinking()
        {
            drinkingItem = null;
            drinkPose = 0f;
            if (firstPersonHands != null) firstPersonHands.SetDrinkingPose(0f);
        }
        private void CreatePrompt()
        {
            promptCanvas = new GameObject("Interaction UI", typeof(RectTransform), typeof(Canvas)); promptCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var label = new GameObject("Prompt Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); label.transform.SetParent(promptCanvas.transform, false);
            var rect = label.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.sizeDelta = new Vector2(300, 65); rect.anchoredPosition = new Vector2(0, -55);
            promptText = label.GetComponent<TextMeshProUGUI>(); OndaSTypography.Apply(promptText); promptText.fontSize = 18; promptText.enableAutoSizing = true; promptText.fontSizeMin = 12; promptText.fontSizeMax = 18; promptText.alignment = TextAlignmentOptions.Center; promptText.raycastTarget = false; promptCanvas.SetActive(false);
        }
    }
}
