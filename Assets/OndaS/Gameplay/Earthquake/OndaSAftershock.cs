using System.Collections.Generic;
using OndaS.Visual;
using OndaS.Gameplay.Items;
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
        private sealed class ShakingProp
        {
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;
            public float releaseAt;
            public float impulse;
            public float slide;
            public Rigidbody body;
            public bool released;
        }
        private readonly List<ShakingProp> props = new List<ShakingProp>();
        private float currentStrength;
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
            var apartment = GameObject.Find("Structure_03/Interior/4rd Floor/Apartment_02/Furniture and Props");
            if (apartment != null) GatherProps(apartment.transform);
            active = true;
            earthquake.BeginAftershock();
            Debug.Log("OndaS: comenzó la réplica del piso 4.", this);
        }

        private void Update()
        {
            if (!active) return;
            if (earthquake == null || player == null || earthquake.IsDefeated) { Finish(); return; }
            if (player.GameplayPaused) return;
            elapsed += Time.deltaTime;
            bool sheltered = player.IsSheltering;
            if (sheltered) { hasSheltered = true; exposed = 0f; }
            else exposed += Time.deltaTime;
            float remaining = (hasSheltered ? returnToShelterSeconds : initialShelterSeconds) - exposed;
            float t = Mathf.Clamp01(elapsed / duration);
            float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .1f)) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / .2f));
            float strength = shakeStrength * envelope * (.65f + .35f * Mathf.PerlinNoise(elapsed * .55f, 3f));
            earthquake.UpdateAftershock(elapsed, strength, sheltered, sheltered ? duration - elapsed : remaining);
            currentStrength = strength;
            AnimateProps();
            if (!sheltered && remaining <= 0f) { earthquake.DefeatByAftershock(); Finish(); return; }
            if (elapsed >= duration) Finish();
        }

        private void GatherProps(Transform apartment)
        {
            // Explicit paths prevent the two shelter tables or unrelated items from becoming dynamic.
            AddProp(apartment, "Living Room/Props/Canvas_Painting_02", 3f, .5f);
            AddProp(apartment, "Living Room/Props/Canvas_Painting_03", 7f, .5f);
            AddProp(apartment, "Living Room/Props/Lamp_Floor_Apt_02", 5f, .45f);
            AddProp(apartment, "Living Room/Props/TV_Apt_01", 9f, .5f);
            AddProp(apartment, "Living Room/Props/GameController_Apt_01_01", 4f, .4f);
            AddProp(apartment, "Living Room/Props/GameConsole_Apt_01", 10f, .4f);
            AddProp(apartment, "Living Room/Props/Sculpture_apt_02_01", 6f, .5f);
            AddProp(apartment, "Living Room/Props/Mech_apt_01", 12f, .4f);
            AddProp(apartment, "Living Room/Props/Trash_apt_01", 8f, .35f);
            AddProp(apartment, "Living Room/Furniture/Chair_Apt_01", 11f, .25f);
            AddProp(apartment, "Living Room/Furniture/Chair_Apt_01 (1)", 14f, .25f);
            AddProp(apartment, "Living Room/Furniture/Table_Side_Apt_01", 13f, .25f);
        }

        private void AddProp(Transform apartment, string path, float releaseAt, float impulse)
        {
            var target = apartment.Find(path);
            if (target == null || !target.gameObject.activeInHierarchy || target.GetComponentInParent<Rigidbody>() != null || target.GetComponentInChildren<Rigidbody>(true) != null ||
                target.GetComponentInParent<OndaSCollectible>() != null || target.GetComponentInChildren<OndaSCollectible>(true) != null ||
                target.GetComponentInParent<OndaSBackpack>() != null || target.GetComponentInChildren<OndaSBackpack>(true) != null) return;
            props.Add(new ShakingProp { transform = target, position = target.localPosition, rotation = target.localRotation,
                releaseAt = releaseAt, impulse = impulse, slide = path.Contains("Furniture/") ? .015f : .025f });
        }

        private void AnimateProps()
        {
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                if (prop.transform == null || prop.released) continue;
                if (elapsed >= prop.releaseAt)
                {
                    prop.transform.localPosition = prop.position;
                    prop.transform.localRotation = prop.rotation;
                    Release(prop, i);
                    continue;
                }
                float sway = Mathf.Sin(elapsed * (5f + i * .13f) + i) * currentStrength * 3f;
                prop.transform.localRotation = prop.rotation * Quaternion.Euler(sway * .4f, 0f, sway);
                prop.transform.localPosition = prop.position + new Vector3(Mathf.Sin(elapsed * 6f + i), 0f, Mathf.Cos(elapsed * 5f + i)) * (prop.slide * currentStrength);
            }
        }

        private static void Release(ShakingProp prop, int index)
        {
            prop.released = true;
            var go = prop.transform.gameObject;
            OndaSFirstEarthquake.PrepareDynamicColliders(go);
            var body = go.AddComponent<Rigidbody>();
            prop.body = body;
            body.mass = go.name.Contains("Chair") || go.name.Contains("Table") ? 4f : 1f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            Vector3 direction = new Vector3(Mathf.Sin(index * 2.1f), .12f, Mathf.Cos(index * 1.7f)).normalized;
            body.AddForce(direction * prop.impulse, ForceMode.VelocityChange);
            body.AddTorque(new Vector3(direction.z, .15f, -direction.x) * prop.impulse, ForceMode.VelocityChange);
        }

        private void FixedUpdate()
        {
            if (!active) return;
            var acceleration = new Vector3(Mathf.Sin(elapsed * 5.5f), 0f, Mathf.Sin(elapsed * 4.3f + 1.2f)) * (currentStrength * 5f);
            foreach (var prop in props)
                if (prop.body != null && !prop.body.isKinematic)
                {
                    prop.body.AddForce(acceleration, ForceMode.Acceleration);
                    prop.body.AddTorque(new Vector3(acceleration.z, 0f, -acceleration.x) * .12f, ForceMode.Acceleration);
                }
        }

        private void Finish()
        {
            active = false; finished = true;
            if (earthquake != null) earthquake.EndAftershock();
            currentStrength = 0f;
            foreach (var prop in props)
                if (!prop.released && prop.transform != null)
                { prop.transform.localPosition = prop.position; prop.transform.localRotation = prop.rotation; }
        }

        private void OnDestroy()
        {
            if (zone != null) zone.PlayerEntered -= Begin;
            if (active) Finish();
        }
    }
}
