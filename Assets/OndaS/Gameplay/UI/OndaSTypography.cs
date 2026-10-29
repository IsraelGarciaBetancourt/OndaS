using TMPro;
using UnityEngine;

namespace OndaS.Gameplay.UI
{
    [CreateAssetMenu(menuName = "OndaS/UI/Typography")]
    public sealed class OndaSTypography : ScriptableObject
    {
        [SerializeField] private TMP_FontAsset regular;
        [SerializeField] private TMP_FontAsset slanted;

        private static OndaSTypography instance;
        public static OndaSTypography Instance => instance != null
            ? instance : instance = Resources.Load<OndaSTypography>("OndaSTypography");
        public TMP_FontAsset Regular => regular;
        public TMP_FontAsset Slanted => slanted;

        public static void Apply(TMP_Text text, bool useSlanted = false)
        {
            var typography = Instance;
            if (typography != null)
            {
                var font = useSlanted ? typography.Slanted : typography.Regular;
                if (font != null) text.font = font;
            }
            text.color = new Color32(255, 218, 65, 255);
            if (text.GetComponent<OndaSScribbleText>() == null)
                text.gameObject.AddComponent<OndaSScribbleText>();
        }
    }
}
