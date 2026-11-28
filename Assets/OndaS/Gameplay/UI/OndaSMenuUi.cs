using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OndaS.Gameplay.UI
{
    internal static class OndaSMenuUi
    {
        public static readonly Color Paper = new Color32(225, 218, 200, 255);
        public static readonly Color Red = new Color32(170, 53, 45, 255);

        public static void AddVolumeSlider(RectTransform parent, Vector2 position)
        {
            AddSlider(parent, "Volumen general", position, 0f, 1f, OndaSGameSettings.MasterVolume,
                value => OndaSGameSettings.MasterVolume = value, value => Mathf.RoundToInt(value * 100) + "%");
        }

        public static void AddSensitivitySlider(RectTransform parent, Vector2 position, Action<float> apply = null)
        {
            AddSlider(parent, "Sensibilidad del ratón", position,
                OndaSGameSettings.MinSensitivity, OndaSGameSettings.MaxSensitivity, OndaSGameSettings.GetSensitivity(),
                value =>
                {
                    OndaSGameSettings.SetSensitivity(value);
                    apply?.Invoke(value);
                }, value => Mathf.RoundToInt(value * 1000).ToString());
        }

        private static void AddSlider(RectTransform parent, string name, Vector2 position,
            float min, float max, float value, Action<float> apply, Func<float, string> format)
        {
            var root = Image(parent, name, position, new Vector2(600, 52), new Color(0, 0, 0, .35f));
            var slider = root.gameObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            var track = Image(root.rectTransform, "Pista", new Vector2(18, -23), new Vector2(450, 6),
                new Color32(90, 84, 78, 255));
            var fill = Image(track.rectTransform, "Nivel", Vector2.zero, Vector2.zero, Red, true);
            slider.fillRect = fill.rectTransform;

            var handleArea = new GameObject("Área selector", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.SetParent(track.transform, false);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = handleArea.offsetMax = Vector2.zero;
            var knob = Image(handleArea, "Selector", Vector2.zero, new Vector2(14, 26), Paper);
            knob.rectTransform.anchorMin = knob.rectTransform.anchorMax = new Vector2(0, .5f);
            knob.rectTransform.pivot = new Vector2(.5f, .5f);
            slider.handleRect = knob.rectTransform;
            slider.targetGraphic = knob;
            slider.value = value;
            var number = Text(root.rectTransform, "Valor", format(value), new Vector2(490, -7),
                new Vector2(100, 40), 28, Paper);
            slider.onValueChanged.AddListener(next =>
            {
                number.text = format(next);
                apply(next);
            });
        }

        public static Image Image(RectTransform parent, string name, Vector2 position, Vector2 size, Color color,
            bool stretch = false)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            var rect = image.rectTransform;
            rect.SetParent(parent, false);
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            image.color = color;
            return image;
        }

        public static TextMeshProUGUI Text(RectTransform parent, string name, string value,
            Vector2 position, Vector2 size, float fontSize, Color color)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI))
                .GetComponent<TextMeshProUGUI>();
            var rect = text.rectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            OndaSTypography.Apply(text);
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
    }
}
