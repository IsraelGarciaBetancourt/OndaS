using OndaS.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OndaS.Gameplay.Earthquake
{
    // The two surfaces are resolved only in the playable eighth-floor apartment.
    internal sealed class OndaSShelterInteraction
    {
        private const float Distance = 2.5f;
        private const float ShelterHeight = 0.75f;
        private readonly Transform player;
        private readonly Camera camera;
        private readonly OndaSLookDevWalkthrough movement;
        private readonly CharacterController controller;
        private readonly Collider[] tops = new Collider[2];

        public OndaSShelterInteraction(Transform player, Camera camera, OndaSLookDevWalkthrough movement)
        {
            this.player = player;
            this.camera = camera;
            this.movement = movement;
            controller = player.GetComponent<CharacterController>();
            var furniture = GameObject.Find("Structure_03/Interior/8th Floor/Apartment_01/Furniture and Props/Living Room/Furniture");
            if (furniture == null) return;
            var table = furniture.transform.Find("Table_Apt_01/Table Top");
            var desk = furniture.transform.Find("ComputerTable_Apt_01");
            if (table != null) tops[0] = table.GetComponent<Collider>();
            if (desk != null) tops[1] = desk.GetComponent<Collider>();
        }

        public bool Update(Keyboard keyboard, out string prompt)
        {
            prompt = null;
            if (movement == null || controller == null || camera == null) return false;
            if (movement.IsSheltering)
            {
                prompt = "[E] Salir del refugio";
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame && !movement.LeaveShelter())
                    prompt = "La salida está bloqueada";
                return true;
            }
            var ray = new Ray(camera.transform.position, camera.transform.forward);
            foreach (var top in tops)
            {
                if (top == null || !TryGetDestination(ray, top, out var destination)) continue;
                prompt = "[E] Para refugiarte";
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                {
                    movement.EnterShelter(destination, ShelterHeight);
                    prompt = "[E] Salir del refugio";
                }
                return true;
            }
            return false;
        }

        private bool TryGetDestination(Ray ray, Collider top, out Vector3 destination)
        {
            destination = default;
            Bounds bounds = top.bounds;
            float targetY = bounds.min.y - 0.35f;
            if (ray.direction.y >= -0.02f) return false;
            float distance = (targetY - ray.origin.y) / ray.direction.y;
            if (distance <= 0f || distance > Distance) return false;
            Vector3 aim = ray.GetPoint(distance);
            float radius = controller.radius;
            if (aim.x < bounds.min.x + radius || aim.x > bounds.max.x - radius ||
                aim.z < bounds.min.z + radius || aim.z > bounds.max.z - radius) return false;
            foreach (var hit in Physics.RaycastAll(ray, distance - 0.03f, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.collider.transform.IsChildOf(player)) return false;

            float floorY = float.NegativeInfinity;
            foreach (var hit in Physics.RaycastAll(aim, Vector3.down, 2f, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.collider.transform.IsChildOf(player) && hit.normal.y > 0.6f)
                    floorY = Mathf.Max(floorY, hit.point.y);
            if (float.IsNegativeInfinity(floorY) || bounds.min.y - floorY < ShelterHeight + 0.02f) return false;
            float bottomOffset = controller.center.y - controller.height * 0.5f;
            destination = new Vector3(aim.x, floorY - bottomOffset + 0.03f, aim.z);
            if (Mathf.Abs(destination.y - player.position.y) > 0.35f) return false;
            float r = Mathf.Max(0.01f, radius - 0.02f);
            Vector3 bottom = destination + Vector3.up * (bottomOffset + r + 0.02f);
            Vector3 upper = destination + Vector3.up * (bottomOffset + ShelterHeight - r - 0.02f);
            foreach (var hit in Physics.OverlapCapsule(bottom, upper, r, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(player)) return false;
            Vector3 delta = destination - player.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > 0.001f)
            {
                foreach (var hit in Physics.CapsuleCastAll(bottom - delta, upper - delta, r, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.collider.transform.IsChildOf(player)) return false;
            }
            return true;
        }
    }
}
