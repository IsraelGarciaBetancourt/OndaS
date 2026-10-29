using UnityEngine;

namespace OndaS.Gameplay.Items
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public sealed class OndaSBackpack : MonoBehaviour
    {
        public const int Capacity = 4;
        [SerializeField] private Sprite icon;
        [SerializeField] private Vector3 equippedPosition = new Vector3(0f, 0.1f, -0.23f);
        [SerializeField] private Vector3 equippedRotation = Vector3.zero;
        private readonly OndaSCollectible[] slots = new OndaSCollectible[Capacity];
        private Collider[] colliders;
        private Renderer[] renderers;
        private bool[] rendererStates;
        private Rigidbody body;
        private Vector3 originalScale;
        public bool IsEquipped { get; private set; }
        public bool WasEquipped { get; private set; }
        public Sprite Icon => icon;
        public OndaSCollectible GetItem(int index) => index >= 0 && index < Capacity ? slots[index] : null;
        internal void SetItem(int index, OndaSCollectible item) { slots[index] = item; }
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>(true);
            renderers = GetComponentsInChildren<Renderer>(true);
            rendererStates = new bool[renderers.Length];
            originalScale = transform.lossyScale;
        }
        public void Equip(Transform player)
        {
            IsEquipped = true;
            WasEquipped = true;
            body.isKinematic = true;
            foreach (var c in colliders) if (c != null) c.enabled = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                rendererStates[i] = renderers[i].enabled;
                renderers[i].enabled = false;
            }
            transform.SetParent(player, false);
            transform.localPosition = equippedPosition;
            transform.localRotation = Quaternion.Euler(equippedRotation);
            transform.localScale = originalScale;
        }
        public void Drop(Vector3 position)
        {
            transform.SetParent(null, false);
            transform.SetPositionAndRotation(position, Quaternion.identity);
            transform.localScale = originalScale;
            IsEquipped = false;
            foreach (var c in colliders) if (c != null) c.enabled = true;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].enabled = rendererStates[i];
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}
