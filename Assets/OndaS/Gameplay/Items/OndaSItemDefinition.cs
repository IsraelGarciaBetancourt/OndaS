using UnityEngine;

namespace OndaS.Gameplay.Items
{
    public enum OndaSItemCategory { Useful, Distracting }

    [CreateAssetMenu(menuName = "OndaS/Items/Item")]
    public sealed class OndaSItemDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private OndaSItemCategory category;
        [SerializeField, Min(0)] private float weightKg = 1f;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private Sprite icon;

        public string DisplayName => displayName;
        public OndaSItemCategory Category => category;
        public float WeightKg => weightKg;
        public GameObject WorldPrefab => worldPrefab;
        public Sprite Icon => icon;
    }
}
