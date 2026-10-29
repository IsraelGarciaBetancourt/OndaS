using OndaS.Visual;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OndaS.Gameplay.UI
{
    public sealed class OndaSStaminaHud : MonoBehaviour
    {
        private OndaSLookDevWalkthrough player;
        private TextMeshProUGUI label;
        private Image fill;
        private int shownPercent = -1;
        private bool shownExhausted;

        public void Initialize(OndaSLookDevWalkthrough source)
        {
            player = source;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var textObject = new GameObject("Stamina label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = textRect.anchorMax = new Vector2(1, 0);
            textRect.pivot = new Vector2(1, 0.5f);
            textRect.anchoredPosition = new Vector2(-50, 133);
            textRect.sizeDelta = new Vector2(310, 40);
            label = textObject.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(label);
            label.fontSize = 27;
            label.alignment = TextAlignmentOptions.Right;
            label.raycastTarget = false;

            var track = MakeBar("Stamina track", transform, new Color(1f, 0.85f, 0.25f, 0.25f));
            var rect = track.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-50, 105);
            rect.sizeDelta = new Vector2(310, 8);
            fill = MakeBar("Stamina fill", track.transform, new Color32(255, 218, 65, 255));
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            Refresh();
        }

        private static Image MakeBar(string name, Transform parent, Color color)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            var image = child.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void LateUpdate()
        {
            if (player != null) Refresh();
        }

        private void Refresh()
        {
            float fraction = player.MaxStamina > 0f ? Mathf.Clamp01(player.Stamina / player.MaxStamina) : 0f;
            fill.rectTransform.anchorMax = new Vector2(fraction, 1f);
            int percent = Mathf.RoundToInt(fraction * 100f);
            if (percent == shownPercent && player.IsExhausted == shownExhausted) return;
            shownPercent = percent;
            shownExhausted = player.IsExhausted;
            label.text = (shownExhausted ? "AGOTADO  " : "ESTAMINA  ") + percent + "%";
        }
    }
}
