using System.Collections.Generic;
using OndaS.Visual;
using UnityEngine;

namespace OndaS.Gameplay.Earthquake
{
    [RequireComponent(typeof(OndaSAftershockZone))]
    public sealed class OndaSAftershock : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float duration = 22f;
        [SerializeField, Min(.1f)] private float initialShelterSeconds = 15f;
        [SerializeField, Min(.1f)] private float returnToShelterSeconds = 3f;
        [SerializeField, Range(0f, 1f)] private float shakeStrength = .85f;
        private readonly List<Transform> props = new List<Transform>();
        private readonly List<Quaternion> restingRotations = new List<Quaternion>();
        private OndaSAftershockZone zone;
        private OndaSFirstEarthquake earthquake;
        private OndaSLookDevWalkthrough player;
        private float elapsed, exposed;
        private bool active, finished, hasSheltered;
        public bool IsActive => active;
        public bool HasFinished => finished;

        private void Awake()
        {
            zone = GetComponent<OndaSAftershockZone>();
            earthquake = FindFirstObjectByType<OndaSFirstEarthquake>();
            player = FindFirstObjectByType<OndaSLookDevWalkthrough>();
            zone.PlayerEntered += Begin;
        }

        private void Begin()
        {
            if (finished || active || earthquake == null || player == null || !earthquake.HasFinished) return;
            var apartment = GameObject.Find("Structure_03/Interior/4rd Floor/Apartment_02/Furniture and Props/Living Room/Props");
            if (apartment != null)
            {
                // Small decorations sway; shelter furniture and its colliders stay still.
                foreach (string name in new[] { "Canvas_Painting_01", "Canvas_Painting_02", "Wall_Mirror_01", "Lamp_Apt_01" })
                {
                    var prop = apartment.transform.Find(name);
                    if (prop == null) continue;
                    props.Add(prop); restingRotations.Add(prop.localRotation);
                }
            }
            active = true;
            earthquake.BeginAftershock();
            Debug.Log("OndaS: comenzó la réplica del piso 4.", this);
        }

        private void Update()
        {
            if (!active) return;
            if (earthquake == null || player == null || earthquake.IsDefeated) { Finish(); return; }
            elapsed += Time.deltaTime;
            bool sheltered = player.IsSheltering;
            if (sheltered) { hasSheltered = true; exposed = 0f; }
            else exposed += Time.deltaTime;
            float remaining = (hasSheltered ? returnToShelterSeconds : initialShelterSeconds) - exposed;
            float t = Mathf.Clamp01(elapsed / duration);
            float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .1f)) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / .2f));
            float strength = shakeStrength * envelope * (.65f + .35f * Mathf.PerlinNoise(elapsed * .55f, 3f));
            earthquake.UpdateAftershock(elapsed, strength, sheltered, sheltered ? duration - elapsed : remaining);
            for (int i = 0; i < props.Count; i++)
                if (props[i] != null) props[i].localRotation = restingRotations[i] * Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * (5f + i * .3f) + i) * strength * 3f);
            if (!sheltered && remaining <= 0f) { earthquake.DefeatByAftershock(); Finish(); return; }
            if (elapsed >= duration) Finish();
        }

        private void Finish()
        {
            active = false; finished = true;
            if (earthquake != null) earthquake.EndAftershock();
            for (int i = 0; i < props.Count; i++) if (props[i] != null) props[i].localRotation = restingRotations[i];
        }

        private void OnDestroy()
        {
            if (zone != null) zone.PlayerEntered -= Begin;
            if (active) Finish();
        }
    }
}
