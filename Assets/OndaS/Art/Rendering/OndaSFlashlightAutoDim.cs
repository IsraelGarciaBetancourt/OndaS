using UnityEngine;

namespace OndaS.Visual
{
    // Runs on the light so it also works while the player controller is disabled for close-ups.
    public sealed class OndaSFlashlightAutoDim : MonoBehaviour
    {
        private const float FullIntensity = 14f;
        private const float NearIntensity = 0.25f;
        private const float FullDistance = 2.5f;
        private const float ProbeRadius = 0.12f;
        private readonly RaycastHit[] hits = new RaycastHit[48];
        private readonly Collider[] overlaps = new Collider[48];
        private Light flashlight;
        private Transform player;

        public void Initialize(Light source, Transform owner)
        {
            flashlight = source;
            player = owner;
        }

        private void LateUpdate()
        {
            if (flashlight == null || !flashlight.enabled) return;
            float distance = FullDistance;
            int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, ProbeRadius, overlaps, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < overlapCount; i++)
                if (IsSceneCollider(overlaps[i])) { distance = 0f; break; }

            int count = Physics.SphereCastNonAlloc(transform.position, ProbeRadius, transform.forward, hits, FullDistance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (IsSceneCollider(hits[i].collider)) distance = Mathf.Min(distance, hits[i].distance + ProbeRadius);

            float fraction = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, FullDistance, distance));
            float target = Mathf.Lerp(NearIntensity, FullIntensity, fraction);
            // Reduce glare immediately; restore reach gradually as the player looks away.
            flashlight.intensity = target < flashlight.intensity
                ? target : Mathf.Lerp(flashlight.intensity, target, 1f - Mathf.Exp(-5f * Time.deltaTime));
        }

        private bool IsSceneCollider(Collider other)
        {
            return other != null && (player == null || !other.transform.IsChildOf(player));
        }
    }
}
