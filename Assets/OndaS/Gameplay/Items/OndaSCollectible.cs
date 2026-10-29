using UnityEngine;

namespace OndaS.Gameplay.Items
{
    public sealed class OndaSCollectible : MonoBehaviour
    {
        [SerializeField] private OndaSItemDefinition item;
        public OndaSItemDefinition Item => item;
    }
}
