using System;
using OndaS.Visual;
using UnityEngine;

namespace OndaS.Gameplay.Earthquake
{
    // Resize the BoxCollider or move this object to choose where the event begins.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class OndaSAftershockZone : MonoBehaviour
    {
        [SerializeField] private OndaSFirstEarthquake firstEarthquake;
        public event Action PlayerEntered;
        public bool HasTriggered { get; private set; }

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            if (firstEarthquake == null) firstEarthquake = FindFirstObjectByType<OndaSFirstEarthquake>();
        }

        private void OnTriggerEnter(Collider other) => TryTrigger(other);
        private void OnTriggerStay(Collider other) => TryTrigger(other);

        private void TryTrigger(Collider other)
        {
            if (HasTriggered || firstEarthquake == null || !firstEarthquake.HasFinished || PlayerEntered == null) return;
            if (other.GetComponentInParent<OndaSLookDevWalkthrough>() == null) return;
            HasTriggered = true;
            PlayerEntered.Invoke();
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, .32f, .08f, .2f);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(1f, .5f, .12f, .9f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
