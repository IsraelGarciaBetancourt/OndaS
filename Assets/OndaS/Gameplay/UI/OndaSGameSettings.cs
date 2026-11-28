using UnityEngine;

namespace OndaS.Gameplay.UI
{
    internal static class OndaSGameSettings
    {
        public const float MinSensitivity = 0.03f;
        public const float MaxSensitivity = 0.30f;
        private const string VolumeKey = "OndaS.MasterVolume";
        private const string SensitivityKey = "OndaS.MouseSensitivity";

        public static float MasterVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
            set
            {
                AudioListener.volume = value;
                PlayerPrefs.SetFloat(VolumeKey, value);
                PlayerPrefs.Save();
            }
        }

        public static float GetSensitivity(float defaultValue = 0.1f) =>
            Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, defaultValue), MinSensitivity, MaxSensitivity);

        public static void SetSensitivity(float value)
        {
            PlayerPrefs.SetFloat(SensitivityKey, value);
            PlayerPrefs.Save();
        }
    }
}
