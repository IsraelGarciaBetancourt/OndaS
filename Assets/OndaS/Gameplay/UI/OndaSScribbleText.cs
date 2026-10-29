using TMPro;
using UnityEngine;

namespace OndaS.Gameplay.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class OndaSScribbleText : MonoBehaviour
    {
        [SerializeField, Min(0)] private float displacement = 1f;
        [SerializeField, Range(0, 1)] private float cornerDistortion = 0.45f;
        [SerializeField, Min(0.1f)] private float drawingsPerSecond = 8f;
        private TMP_Text text;
        private int drawing = -1;

        private void Awake() => text = GetComponent<TMP_Text>();

        private void OnEnable()
        {
            if (text == null) text = GetComponent<TMP_Text>();
            text.OnPreRenderText += Animate;
            drawing = -1;
            RefreshText();
        }

        private void OnDisable()
        {
            if (text == null) return;
            text.OnPreRenderText -= Animate;
            RefreshText();
        }

        private void Update()
        {
            int next = Mathf.FloorToInt(Time.unscaledTime * drawingsPerSecond);
            if (next == drawing) return;
            drawing = next;
            RefreshText();
        }

        private void RefreshText()
        {
            text.havePropertiesChanged = true;
            text.SetVerticesDirty();
        }

        private void Animate(TMP_TextInfo info)
        {
            for (int i = 0; i < info.characterCount; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible) continue;
                var vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                var offset = new Vector3(Noise(i * 41, drawing), Noise(i * 41 + 7, drawing), 0) * displacement;
                for (int corner = 0; corner < 4; corner++)
                {
                    int seed = i * 41 + corner * 11;
                    vertices[character.vertexIndex + corner] += offset + new Vector3(
                        Noise(seed, drawing) * displacement,
                        Noise(seed + 7, drawing) * displacement, 0) * cornerDistortion;
                }
            }
        }

        private static float Noise(int seed, int frame)
        {
            unchecked
            {
                uint value = (uint)(seed * 73856093) ^ (uint)(frame * 19349663);
                value ^= value >> 16;
                value *= 0x7feb352d;
                value ^= value >> 15;
                return (value & 65535) / 32767.5f - 1f;
            }
        }
    }
}
