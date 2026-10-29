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
        private Image[] icons;
        private Image[] frames;
        private int selected;
        public int Selected => selected;
        public void Initialize(OndaSInventory source)
        {
            inventory = source;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            labels = new TextMeshProUGUI[3]; icons = new Image[3]; frames = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var slot = new GameObject("Slot " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                slot.transform.SetParent(transform, false);
                var rect = slot.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2((i - 1) * 165, 93);
                rect.sizeDelta = new Vector2(150, 105);
                frames[i] = slot.GetComponent<Image>(); frames[i].raycastTarget = false;
                var iconObject = new GameObject("Icono", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObject.transform.SetParent(slot.transform, false);
                var ir = iconObject.GetComponent<RectTransform>(); ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.65f); ir.sizeDelta = new Vector2(48, 48);
                icons[i] = iconObject.GetComponent<Image>(); icons[i].preserveAspect = true; icons[i].raycastTarget = false;
                var textObject = new GameObject("Nombre", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(slot.transform, false);
                var tr = textObject.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = new Vector2(4, 2); tr.offsetMax = new Vector2(-4, -2);
                labels[i] = textObject.GetComponent<TextMeshProUGUI>(); OndaSTypography.Apply(labels[i]);
                labels[i].fontSize = 24; labels[i].enableAutoSizing = true; labels[i].fontSizeMin = 14; labels[i].fontSizeMax = 24;
                labels[i].alignment = TextAlignmentOptions.Bottom; labels[i].raycastTarget = false;
            }
            inventory.Changed += Refresh; Refresh();
        }
        public void Select(int slot) { if (slot >= 0 && slot < 3) { selected = slot; Refresh(); } }
        private void Refresh()
        {
            for (int i = 0; i < 3; i++)
            {
                var item = i < 2 ? inventory.GetItem(i) : null;
                var bag = i == 2 ? inventory.Backpack : null;
                Sprite icon = item != null && item.Item != null ? item.Item.Icon : bag != null ? bag.Icon : null;
                icons[i].sprite = icon; icons[i].enabled = icon != null;
                var name = i == 2 ? (bag != null ? "MOCHILA" : "SIN MOCHILA") : item != null && item.Item != null ? item.Item.DisplayName : "VACÍO";
                labels[i].text = (i + 1) + "  " + name;
                frames[i].color = i == selected ? new Color(0.35f, 0.29f, 0.12f, 0.8f) : new Color(0.10f, 0.10f, 0.08f, 0.55f);
            }
        }
        private void OnDestroy() { if (inventory != null) inventory.Changed -= Refresh; }
    }
}
