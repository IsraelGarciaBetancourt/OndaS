using OndaS.Gameplay.Earthquake;
using OndaS.Visual;
using UnityEngine;

namespace OndaS.Gameplay.Electrical
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class OndaSExposedWires : MonoBehaviour
    {
        [SerializeField] private OndaSFuseBox fuseBox;
        [SerializeField] private OndaSFirstEarthquake earthquake;
        [SerializeField] private ParticleSystem sparks;
        [SerializeField] private Light sparkLight;
        [SerializeField] private AudioSource buzz;

        private BoxCollider dangerZone;
        private CharacterController player;
        private AudioClip generatedBuzz;
        private bool energized;
        private float nextSpark;
        private float lightUntil;

        public bool IsEnergized => fuseBox != null && earthquake != null && earthquake.HasFinished && !fuseBox.IsPowerIsolated;

        private void Awake()
        {
            dangerZone = GetComponent<BoxCollider>();
            dangerZone.isTrigger = true;
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var movement = FindAnyObjectByType<OndaSLookDevWalkthrough>();
            if (movement != null) player = movement.GetComponent<CharacterController>();
            if (buzz != null)
            {
                generatedBuzz = CreateBuzz();
                buzz.clip = generatedBuzz;
            }
        }

        private void OnEnable()
        {
            if (fuseBox != null) fuseBox.PowerIsolationChanged += OnPowerChanged;
            ApplyPower(IsEnergized);
        }

        private void OnDisable()
        {
            if (fuseBox != null) fuseBox.PowerIsolationChanged -= OnPowerChanged;
            ApplyPower(false);
        }

        private void OnDestroy()
        {
            if (generatedBuzz != null) Destroy(generatedBuzz);
        }

        private void OnPowerChanged(bool isolated) => ApplyPower(IsEnergized);
        private void OnTriggerEnter(Collider other) => TryShock(other);
        private void OnTriggerStay(Collider other) => TryShock(other);

        private void Update()
        {
            if (energized != IsEnergized) ApplyPower(IsEnergized);
            if (!energized) return;

            if (Time.time >= nextSpark)
            {
                if (sparks != null) sparks.Emit(Random.Range(3, 8));
                lightUntil = Time.time + Random.Range(0.045f, 0.1f);
                nextSpark = Time.time + Random.Range(0.3f, 1.1f);
            }
            if (sparkLight != null)
            {
                sparkLight.enabled = Time.time < lightUntil;
                if (sparkLight.enabled) sparkLight.intensity = Random.Range(1.1f, 2f);
            }
            if (player != null && player.enabled && dangerZone.enabled &&
                Physics.ComputePenetration(dangerZone, transform.position, transform.rotation,
                    player, player.transform.position, player.transform.rotation, out _, out _))
                TryShock(player);
        }

        private void TryShock(Collider other)
        {
            if (!IsEnergized || !(other is CharacterController)) return;
            var movement = other.GetComponent<OndaSLookDevWalkthrough>();
            if (movement == null || !movement.isActiveAndEnabled || movement.GameplayPaused) return;
            earthquake.DefeatByElectricalHazard();
        }

        private void ApplyPower(bool value)
        {
            energized = value;
            if (sparkLight != null) sparkLight.enabled = false;
            if (sparks != null)
            {
                if (value) sparks.Play();
                else sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (buzz != null)
            {
                if (value && !buzz.isPlaying) buzz.Play();
                else if (!value) buzz.Stop();
            }
            if (value) nextSpark = Time.time + 0.1f;
        }

        private static AudioClip CreateBuzz()
        {
            const int sampleRate = 16000;
            var samples = new float[sampleRate];
            var random = new System.Random(518);
            float noise = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / sampleRate;
                noise = noise * 0.85f + ((float)random.NextDouble() * 2f - 1f) * 0.15f;
                float crackle = Mathf.PerlinNoise(t * 9f, 0.3f) > 0.63f ? noise * 0.65f : noise * 0.06f;
                samples[i] = Mathf.Sin(t * 2f * Mathf.PI * 60f) * 0.12f + crackle;
            }
            var clip = AudioClip.Create("OndaS - zumbido eléctrico", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
