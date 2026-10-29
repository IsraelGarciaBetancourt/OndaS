using OndaS.Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OndaS.Gameplay.Items
{
    public sealed class OndaSInventoryHud : MonoBehaviour
    {
        private OndaSInventory inventory;
        private TextMeshProUGUI[] labels;
        private TextMeshProUGUI weightLabel;
        private int selected;

        public void Initialize(OndaSInventory source)
        {
            inventory = source;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            labels = new TextMeshProUGUI[OndaSInventory.Capacity];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = MakeLabel("Espacio " + (i + 1), new Vector2((i - 2) * 210, 70), new Vector2(205, 78), 30);
            }
            weightLabel = MakeLabel("Peso", new Vector2(0, 140), new Vector2(1100, 42), 27);
            inventory.Changed += Refresh;
            Refresh();
        }

        public void Select(int slot)
        {
            if (slot < 0 || slot >= OndaSInventory.Capacity) return;
            selected = slot;
            Refresh();
        }

        public int Selected => selected;

        private TextMeshProUGUI MakeLabel(string labelName, Vector2 position, Vector2 size, float fontSize)
        {
            var child = new GameObject(labelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            child.transform.SetParent(transform, false);
            var rect = child.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = child.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(text);
            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = 20;
            text.fontSizeMax = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private void Refresh()
        {
            for (int i = 0; i < labels.Length; i++)
            {
                var item = inventory.GetItem(i);
                labels[i].text = (selected == i ? "> " : "") + (i + 1) + "\n" + (item != null ? item.DisplayName : "—");
            }
            weightLabel.text = "MOCHILA  " + inventory.TotalWeightKg.ToString("0.0") + " / " + OndaSInventory.MaxWeightKg.ToString("0.0") + " kg   |   1–5: seleccionar   Q: soltar";
        }

        private void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= Refresh;
        }
    }
}
