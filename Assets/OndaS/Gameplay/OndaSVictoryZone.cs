using System.Collections.Generic;
using OndaS.Gameplay.Electrical;
using OndaS.Gameplay.Gas;
using OndaS.Gameplay.Items;
using OndaS.Gameplay.UI;
using OndaS.Visual;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OndaS.Gameplay
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class OndaSVictoryZone : MonoBehaviour
    {
        private bool won;
        private float previousTimeScale;
        private GameObject victoryCanvas;
        private BoxCollider zone;
        private CharacterController playerController;
        private const int ItemsPerPage = 8;
        private readonly List<string> usefulItems = new List<string>();
        private TextMeshProUGUI itemsText, pageText;
        private int itemPage;

        private void Reset() => ConfigureTrigger();
        private void Awake()
        {
            ConfigureTrigger();
            var player = FindAnyObjectByType<OndaSLookDevWalkthrough>();
            if (player != null) playerController = player.GetComponent<CharacterController>();
        }

        private void ConfigureTrigger()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other) => TryWin(other);
        private void OnTriggerStay(Collider other) => TryWin(other);

        private void TryWin(Collider other)
        {
            if (won || !(other is CharacterController)) return;
            var player = other.GetComponent<OndaSLookDevWalkthrough>();
            if (player == null || !player.isActiveAndEnabled) return;

            won = true;
            previousTimeScale = Time.timeScale;
            var interactor = player.GetComponent<OndaSDoorInteractor>();
            if (interactor != null) interactor.enabled = false;
            player.enabled = false;
            zone.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowVictory(player);
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (!won)
            {
                if (playerController != null && playerController.enabled && zone.enabled &&
                    Physics.ComputePenetration(zone, zone.transform.position, zone.transform.rotation,
                        playerController, playerController.transform.position, playerController.transform.rotation,
                        out _, out _))
                    TryWin(playerController);
                return;
            }
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            int pageCount = Mathf.Max(1, (usefulItems.Count + ItemsPerPage - 1) / ItemsPerPage);
            if (keyboard.leftArrowKey.wasPressedThisFrame && itemPage > 0) { itemPage--; RefreshItems(); }
            if (keyboard.rightArrowKey.wasPressedThisFrame && itemPage + 1 < pageCount) { itemPage++; RefreshItems(); }
            if (!keyboard.rKey.wasPressedThisFrame && !keyboard.enterKey.wasPressedThisFrame && !keyboard.numpadEnterKey.wasPressedThisFrame) return;
            Time.timeScale = previousTimeScale;
            SceneManager.LoadScene(gameObject.scene.path);
        }

        private void ShowVictory(OndaSLookDevWalkthrough player)
        {
            victoryCanvas = new GameObject("OndaS - Victoria", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = victoryCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = victoryCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var background = new GameObject("Fondo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(victoryCanvas.transform, false);
            var rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.015f, 0.015f, 0.012f, 0.98f);
            var inventory = player.GetComponent<OndaSInventory>();
            if (inventory != null)
            {
                var counts = new SortedDictionary<string, int>();
                foreach (var item in inventory.CollectedUsefulItems)
                {
                    if (item == null) continue;
                    string name = string.IsNullOrWhiteSpace(item.DisplayName) ? "Objeto útil" : item.DisplayName;
                    counts.TryGetValue(name, out int count);
                    counts[name] = count + 1;
                }
                foreach (var entry in counts)
                    usefulItems.Add(entry.Key + (entry.Value > 1 ? "  x" + entry.Value : ""));
            }
            var gas = FindFirstObjectByType<OndaSGasValve>();
            var fuseBox = FindFirstObjectByType<OndaSFuseBox>();
            var warmWhite = new Color32(230, 220, 200, 255);
            var gold = new Color32(255, 218, 65, 255);
            CreateText("Título", "¡ESCAPASTE!", new Vector2(0f, 415f), new Vector2(1200f, 100f), 72f, gold);
            CreateText("Mensaje", "RESUMEN DE TU EVACUACIÓN", new Vector2(0f, 335f), new Vector2(1200f, 60f), 28f, warmWhite);
            CreateStatus("Gas", "VÁLVULA DE GAS", gas != null ? (bool?)gas.IsClosed : null, new Vector2(-330f, 230f));
            CreateStatus("Electricidad", "ELECTRICIDAD", fuseBox != null ? (bool?)fuseBox.IsPowerIsolated : null, new Vector2(330f, 230f));
            CreateText("Objetos título", "OBJETOS ÚTILES RECOGIDOS", new Vector2(0f, 125f), new Vector2(1200f, 60f), 30f, gold);
            itemsText = CreateText("Objetos", "", new Vector2(0f, -75f), new Vector2(1100f, 330f), 30f, warmWhite);
            pageText = CreateText("Páginas", "", new Vector2(0f, -290f), new Vector2(1100f, 60f), 24f, warmWhite);
            RefreshItems();
            CreateText("Reiniciar", "[R / ENTER] VOLVER A JUGAR", new Vector2(0f, -415f), new Vector2(1200f, 80f), 34f, gold);
        }

        private void CreateStatus(string name, string label, bool? completed, Vector2 position)
        {
            CreateText(name + " título", label, position + new Vector2(0f, 25f), new Vector2(600f, 50f), 27f, new Color32(230, 220, 200, 255));
            string value = !completed.HasValue ? "SIN DATOS" : completed.Value ? "CERRADA" : "NO CERRADA";
            if (name == "Electricidad" && completed.HasValue) value = completed.Value ? "APAGADA" : "NO APAGADA";
            var color = !completed.HasValue ? new Color32(165, 160, 150, 255) : completed.Value ? new Color32(159, 205, 140, 255) : new Color32(218, 143, 96, 255);
            CreateText(name + " estado", value, position - new Vector2(0f, 20f), new Vector2(600f, 50f), 29f, color);
        }

        private void RefreshItems()
        {
            if (itemsText == null || pageText == null) return;
            int pages = Mathf.Max(1, (usefulItems.Count + ItemsPerPage - 1) / ItemsPerPage);
            if (usefulItems.Count == 0) itemsText.text = "No recogiste objetos útiles.";
            else
            {
                int start = itemPage * ItemsPerPage;
                int count = Mathf.Min(ItemsPerPage, usefulItems.Count - start);
                itemsText.text = string.Join("\n", usefulItems.GetRange(start, count));
            }
            pageText.text = pages > 1 ? "[FLECHAS IZQ / DER]  " + (itemPage + 1) + " / " + pages : "";
        }

        private TextMeshProUGUI CreateText(string name, string value, Vector2 position, Vector2 size, float fontSize, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            obj.transform.SetParent(victoryCanvas.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var text = obj.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(text);
            text.text = value; text.fontSize = fontSize; text.color = color;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            return text;
        }

        private void OnDestroy()
        {
            if (won) Time.timeScale = previousTimeScale;
            if (victoryCanvas != null) Destroy(victoryCanvas);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.15f);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.9f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
