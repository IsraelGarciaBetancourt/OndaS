using UnityEngine;

namespace OndaS.Gameplay.Items
{
    public sealed class OndaSCollectible : MonoBehaviour
    {
        [SerializeField] private OndaSItemDefinition item;
        private Vector3 worldScale;
        private bool cachedScale;
        public OndaSItemDefinition Item => item;
        public void Store(Transform parent)
        {
            if (!cachedScale) { worldScale = transform.lossyScale; cachedScale = true; }
            gameObject.SetActive(false);
            transform.SetParent(parent, false);
        }
        public void Drop(Vector3 position)
        {
            transform.SetParent(null, false);
            transform.SetPositionAndRotation(position, Quaternion.identity);
            if (cachedScale) transform.localScale = worldScale;
            gameObject.SetActive(true);
        }
    }
}
