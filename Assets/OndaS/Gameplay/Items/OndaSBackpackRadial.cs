using OndaS.Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OndaS.Gameplay.Items
{
    public sealed class OndaSBackpackRadial : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public bool Active => gameObject.activeSelf;
        private readonly OndaSRadialWedge[] wedges = new OndaSRadialWedge[5];
        private readonly TextMeshProUGUI[] labels = new TextMeshProUGUI[5];
        private readonly Image[] icons = new Image[5];
        private RectTransform root;
        private Canvas canvas;
        private OndaSBackpack backpack;
        private Vector2 direction;
        private int selected = -1;
        public int Selected => selected;
        public void Initialize()
        {
            canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            var holder = new GameObject("Radial", typeof(RectTransform)); holder.transform.SetParent(transform, false);
            root = holder.GetComponent<RectTransform>(); root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f); root.anchoredPosition = Vector2.zero; root.sizeDelta = new Vector2(410, 410);
            for (int i = 0; i < 5; i++)
            {
                var section = new GameObject("Sección " + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(OndaSRadialWedge));
                section.transform.SetParent(root, false); var sr = section.GetComponent<RectTransform>(); sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one; sr.offsetMin = sr.offsetMax = Vector2.zero;
                wedges[i] = section.GetComponent<OndaSRadialWedge>(); wedges[i].centerAngle = -18 + i * 72; wedges[i].raycastTarget = false;
                float radians = (-18 + i * 72) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * 137;
                var iconObj = new GameObject("Icono", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); iconObj.transform.SetParent(root, false);
                var ir = iconObj.GetComponent<RectTransform>(); ir.anchoredPosition = pos + Vector2.up * 20; ir.sizeDelta = new Vector2(54, 54);
                icons[i] = iconObj.GetComponent<Image>(); icons[i].preserveAspect = true; icons[i].raycastTarget = false;
                var label = new GameObject("Etiqueta", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); label.transform.SetParent(root, false);
                var lr = label.GetComponent<RectTransform>(); lr.anchoredPosition = pos + Vector2.down * 15; lr.sizeDelta = new Vector2(115, 55);
                labels[i] = label.GetComponent<TextMeshProUGUI>(); OndaSTypography.Apply(labels[i]); labels[i].fontSize = 23;
                labels[i].enableAutoSizing = true; labels[i].fontSizeMin = 14; labels[i].fontSizeMax = 23; labels[i].alignment = TextAlignmentOptions.Center; labels[i].raycastTarget = false;
            }
            gameObject.SetActive(false);
        }
        public void Open(OndaSBackpack bag, Camera camera)
        {
            backpack = bag; direction = Vector2.zero; selected = -1; IsOpen = true; gameObject.SetActive(true); Refresh();
        }
        public void Move(Vector2 delta)
        {
            direction = Vector2.ClampMagnitude(direction + delta, 120f);
            int next = direction.magnitude < 20f ? -1 : Mathf.RoundToInt((Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 18f) / 72f);
            next = next < 0 && direction.magnitude >= 20f ? (next % 5 + 5) % 5 : next % 5;
            if (next != selected) { selected = next; Refresh(); }
        }
        public void Close() { IsOpen = false; backpack = null; selected = -1; gameObject.SetActive(false); }
        private void OnDisable() { IsOpen = false; }
        private void Refresh()
        {
            for (int i = 0; i < 5; i++)
            {
                var item = i < 4 && backpack != null ? backpack.GetItem(i) : null;
                var icon = item != null && item.Item != null ? item.Item.Icon : i == 4 && backpack != null ? backpack.Icon : null;
                icons[i].sprite = icon; icons[i].enabled = icon != null;
                labels[i].text = i == 4 ? "EQUIPAR" : item != null && item.Item != null ? item.Item.DisplayName : "VACÍO";
                wedges[i].color = i == selected ? new Color(0.48f, 0.39f, 0.17f, 0.94f) : new Color(0.12f, 0.12f, 0.10f, 0.84f);
            }
        }
    }
    public sealed class OndaSRadialWedge : MaskableGraphic
    {
        public float centerAngle;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); const int segments = 12; float inner = 65f, outer = 196f;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (centerAngle - 34f + 68f * i / segments) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(dir * inner, color, Vector2.zero); vh.AddVert(dir * outer, color, Vector2.zero);
                if (i == 0) continue;
                int p = (i - 1) * 2; vh.AddTriangle(p, p + 1, p + 2); vh.AddTriangle(p + 1, p + 3, p + 2);
            }
        }
    }
}
