using OndaS.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OndaS.Gameplay.Earthquake
{
    internal sealed class OndaSShelterInteraction
    {
        private readonly Transform player;
        private readonly Camera camera;
        private readonly OndaSLookDevWalkthrough movement;
        private readonly OndaSShelterPoint[] points;
        private float exitHintUntil;

        public OndaSShelterInteraction(Transform player, Camera camera, OndaSLookDevWalkthrough movement)
        {
            this.player = player;
            this.camera = camera;
            this.movement = movement;
            points = Object.FindObjectsByType<OndaSShelterPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        public bool Update(Keyboard keyboard, out string prompt)
        {
            prompt = null;
            if (movement == null || camera == null) return false;
            if (movement.IsShelterTransitioning) { prompt = "Refugiándose..."; return true; }
            if (movement.IsSheltering)
            {
                prompt = "[E] Salir del refugio";
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame) movement.LeaveShelter();
                return true;
            }
            if (movement.ShelterExitRequiresMovement && exitHintUntil == 0f) exitHintUntil = Time.unscaledTime + 3f;
            if (Time.unscaledTime < exitHintUntil) { prompt = "Muévete agachado para salir"; return true; }
            foreach (var point in points)
            {
                if (point == null || !point.CanApproach(player, camera)) continue;
                prompt = "[E] Para refugiarte";
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame) { exitHintUntil = 0f; movement.EnterShelter(point); }
                return true;
            }
            return false;
        }
    }
}
