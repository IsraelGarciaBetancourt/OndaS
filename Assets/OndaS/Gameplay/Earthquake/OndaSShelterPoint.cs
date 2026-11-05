using UnityEngine;

namespace OndaS.Gameplay.Earthquake
{
    // Place this object at the entrance, with its blue arrow pointing under the furniture.
    public sealed class OndaSShelterPoint : MonoBehaviour
    {
        [SerializeField, Min(0.2f)] private float approachDistance = 1.25f;
        [SerializeField, Min(0.1f)] private float aimRadius = 0.55f;
        [SerializeField, Min(0.1f)] private float shelterHeight = 0.75f;
        public Vector3 EntryPosition => transform.position;
        public Transform Interior => transform.Find("Interior");
        public float ShelterHeight => shelterHeight;

        public bool CanApproach(Transform player, Camera camera)
        {
            if (Interior == null || player == null || camera == null) return false;
            Vector3 offset = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up);
            Vector3 inward = Vector3.ProjectOnPlane(Interior.position - transform.position, Vector3.up).normalized;
            if (offset.magnitude > approachDistance || inward.sqrMagnitude < 0.01f || Vector3.Dot(offset, inward) > 0.15f) return false;
            Vector3 target = transform.position + Vector3.up * 0.5f;
            Vector3 toTarget = target - camera.transform.position;
            float along = Vector3.Dot(toTarget, camera.transform.forward);
            if (along <= 0f || along > 2.5f || (toTarget - camera.transform.forward * along).sqrMagnitude > aimRadius * aimRadius) return false;
            foreach (var hit in Physics.RaycastAll(camera.transform.position, toTarget.normalized, toTarget.magnitude - 0.03f, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.collider.transform.IsChildOf(player)) return false;
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.15f, 0.22f);
            var inside = Interior;
            if (inside == null) return;
            Vector3 start = transform.position + Vector3.up * 0.15f;
            Vector3 end = inside.position + Vector3.up * 0.15f;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.16f);
            Vector3 direction = Vector3.ProjectOnPlane(end - start, Vector3.up).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction);
            Gizmos.DrawLine(end, end - direction * 0.22f + side * 0.1f);
            Gizmos.DrawLine(end, end - direction * 0.22f - side * 0.1f);
        }
    }
}
