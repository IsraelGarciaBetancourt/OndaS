using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using OndaS.Gameplay.UI;
using OndaS.Gameplay.Items;

namespace OndaS.Gameplay
{
    public sealed class OndaSDoorInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        private OndaSDoor focusedDoor;
        private GameObject promptCanvas;
        private TextMeshProUGUI promptText;
        private OndaSInventory inventory;
        private OndaSInventoryHud inventoryHud;
        private float messageUntil;
        private string message;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            inventory = GetComponent<OndaSInventory>();
            if (inventory == null) inventory = gameObject.AddComponent<OndaSInventory>();
            CreatePrompt();
            var hud = new GameObject("Inventory UI", typeof(RectTransform));
            inventoryHud = hud.AddComponent<OndaSInventoryHud>();
            inventoryHud.Initialize(inventory);
        }

        private void Update()
        {
            focusedDoor = null;
            if (viewCamera == null || Cursor.lockState != CursorLockMode.Locked)
            {
                if (promptCanvas != null) promptCanvas.SetActive(false);
                return;
            }
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) inventoryHud.Select(0);
                if (keyboard.digit2Key.wasPressedThisFrame) inventoryHud.Select(1);
                if (keyboard.digit3Key.wasPressedThisFrame) inventoryHud.Select(2);
                if (keyboard.digit4Key.wasPressedThisFrame) inventoryHud.Select(3);
                if (keyboard.digit5Key.wasPressedThisFrame) inventoryHud.Select(4);
                if (keyboard.qKey.wasPressedThisFrame) DropSelected();
            }
            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out var hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore))
            {
                ShowPrompt(null);
                return;
            }
            var collectible = hit.collider.GetComponentInParent<OndaSCollectible>();
            if (collectible != null && collectible.Item != null)
            {
                var result = inventory.CanAdd(collectible.Item);
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame && result == OndaSAddResult.Added)
                {
                    if (inventory.TryAdd(collectible.Item) == OndaSAddResult.Added)
                    {
                        Destroy(collectible.gameObject);
                        ShowPrompt(null);
                        return;
                    }
                }
                ShowPrompt(result == OndaSAddResult.Full ? "Mochila llena" : result == OndaSAddResult.TooHeavy ? "Excede el peso máximo" : "[E] Recoger " + collectible.Item.DisplayName);
                return;
            }
            focusedDoor = hit.collider.GetComponentInParent<OndaSDoor>();
            if (focusedDoor != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                focusedDoor.Toggle();
            if (focusedDoor != null && focusedDoor.CanInteract && promptCanvas != null)
            {
                ShowPrompt(focusedDoor.IsOpen ? "[E] Cerrar puerta" : "[E] Abrir puerta");
            }
            else ShowPrompt(null);
        }

        private void OnDisable()
        {
            if (promptCanvas != null) promptCanvas.SetActive(false);
            if (inventoryHud != null) inventoryHud.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (inventoryHud != null) inventoryHud.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (promptCanvas != null) Destroy(promptCanvas);
            if (inventoryHud != null) Destroy(inventoryHud.gameObject);
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
            if (item == null) { Notify("Espacio vacío"); return; }
            if (item.WorldPrefab == null) { Notify("No se puede soltar este objeto"); return; }
            Vector3 forward = Vector3.ProjectOnPlane(viewCamera.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
            var origin = viewCamera.transform.position;
            float distance = 1.25f;
            if (Physics.Raycast(origin, forward, out var wall, distance, interactionLayers, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0, wall.distance - 0.3f);
            if (distance < 0.45f) { Notify("No hay espacio para soltar"); return; }
            Vector3 above = origin + forward * distance;
            var hits = Physics.RaycastAll(above, Vector3.down, 3f, interactionLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            RaycastHit ground = default;
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.GetComponentInParent<OndaSCollectible>() != null || hit.normal.y < 0.6f || hit.distance >= nearest) continue;
                nearest = hit.distance;
                ground = hit;
            }
            if (float.IsPositiveInfinity(nearest)) { Notify("No hay suelo para soltar"); return; }
            var dropped = Instantiate(item.WorldPrefab, ground.point + Vector3.up * 0.02f, Quaternion.identity);
            if (dropped == null) { Notify("No se pudo soltar el objeto"); return; }
            inventory.Remove(slot);
            Notify("Soltaste " + item.DisplayName);
        }

        private void Notify(string value)
        {
            message = value;
            messageUntil = Time.unscaledTime + 1.5f;
        }

        private void CreatePrompt()
        {
            promptCanvas = new GameObject("Door Interaction UI", typeof(RectTransform), typeof(Canvas));
            var canvas = promptCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var label = new GameObject("Prompt Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(promptCanvas.transform, false);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240, 34);
            rect.anchoredPosition = new Vector2(0, -45);
            promptText = label.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(promptText);
            promptText.fontSize = 18;
            promptText.enableAutoSizing = true;
            promptText.fontSizeMin = 12;
            promptText.fontSizeMax = 18;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.raycastTarget = false;
            promptCanvas.SetActive(false);
        }
    }
}
