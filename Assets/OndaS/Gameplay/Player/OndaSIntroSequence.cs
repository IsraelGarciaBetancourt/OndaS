using System.Collections;
using OndaS.Gameplay.Earthquake;
using OndaS.Visual;
using UnityEngine;
using UnityEngine.Rendering;

namespace OndaS.Gameplay
{
    // The normal first-person camera is used so the existing PSX post effect stays active.
    [DefaultExecutionOrder(-100)]
    public sealed class OndaSIntroSequence : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float sleepingSeconds = 4f;
        [SerializeField, Min(0.1f)] private float alarmSeconds = 4f;
        [SerializeField, Min(0.1f)] private float wakeSeconds = 3.5f;
        public static bool IsPlaying { get; private set; }
        private Transform phone, screen, cameraTransform, cameraParent, visual;
        private Vector3 cameraLocalPosition, phonePosition;
        private Quaternion cameraLocalRotation;
        private GameObject sleepingPlayer, staminaUi, inventoryUi;
        private OndaSFirstEarthquake earthquake;
        private AudioSource alarm;
        private AudioClip alarmClip;
        private Light screenLight, bedLight;
        private bool completed;

        private void Awake() { IsPlaying = true; }

        private IEnumerator Start()
        {
            // Let the player and earthquake initialize their camera pivot and HUDs.
            yield return null;
            var player = GameObject.Find("Base Scene/First Person Player");
            var phoneObject = GameObject.Find("OndaS - Celular mesa de noche");
            var bed = GameObject.Find("Structure_03/Interior/8th Floor/Apartment_01/Furniture and Props/Bed Room_01/Furniture/Bed_Apt_02");
            earthquake = FindFirstObjectByType<OndaSFirstEarthquake>();
            var camera = player != null ? player.GetComponentInChildren<Camera>(true) : null;
            visual = player != null ? player.transform.Find("Office Worker") : null;
            if (phoneObject == null || bed == null || camera == null || visual == null || earthquake == null)
            { Debug.LogWarning("Intro: faltan objetos de la habitación; se inicia el juego normalmente.", this); Complete(); yield break; }
            phone = phoneObject.transform;
            screen = FindChild(phone, "Pantalla - alerta sísmica");
            if (screen == null) { Debug.LogWarning("Intro: falta la pantalla del celular.", this); Complete(); yield break; }
            cameraTransform = camera.transform; cameraParent = cameraTransform.parent;
            cameraLocalPosition = cameraTransform.localPosition; cameraLocalRotation = cameraTransform.localRotation;
            phonePosition = phone.localPosition;
            screen.gameObject.SetActive(false);
            earthquake.SetIntroHudVisible(false);
            staminaUi = GameObject.Find("Stamina UI"); inventoryUi = GameObject.Find("Inventory UI");
            if (staminaUi != null) staminaUi.SetActive(false);
            if (inventoryUi != null) inventoryUi.SetActive(false);
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            player.transform.SetPositionAndRotation(new Vector3(31.72f, 29f, 8.05f), Quaternion.Euler(0f, 180f, 0f));
            if (controller != null) controller.enabled = true;

            sleepingPlayer = Instantiate(visual.gameObject);
            sleepingPlayer.name = "OndaS - Personaje de introducción";
            var animator = sleepingPlayer.GetComponent<Animator>();
            if (animator != null) { animator.Play("Idle", 0, 0f); animator.Update(0f); animator.enabled = false; }
            foreach (var renderer in sleepingPlayer.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                var lighting = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(lighting);
                lighting.SetFloat("_MinimumLight", .42f);
                renderer.SetPropertyBlock(lighting);
            }
            sleepingPlayer.transform.SetPositionAndRotation(new Vector3(30.50f, 29.28f, 8.7f), Quaternion.Euler(-90f, 180f, 0f));
            var startPosition = sleepingPlayer.transform.position;
            var startRotation = sleepingPlayer.transform.rotation;
            var bedLightObject = new GameObject("OndaS - Luz tenue de introducción");
            bedLightObject.transform.position = new Vector3(30.5f, 30.2f, 9.3f);
            bedLight = bedLightObject.AddComponent<Light>(); bedLight.type = LightType.Point;
            bedLight.range = 3f; bedLight.intensity = 2.5f; bedLight.color = new Color(1f, .8f, .65f);
            CreateAlarm();
            yield return Shot(new Vector3(27.9f, 30.3f, 7.2f), new Vector3(30.4f, 29.1f, 9.7f), sleepingSeconds, false);
            screen.gameObject.SetActive(true);
            if (alarm != null) alarm.Play();
            if (screenLight != null) screenLight.enabled = true;
            yield return Shot(new Vector3(32.28f, 29.65f, 9.90f), new Vector3(31.55f, 29.08f, 9.98f), alarmSeconds, true);
            if (alarm != null) alarm.Stop();
            phone.localPosition = phonePosition;
            if (screenLight != null) screenLight.enabled = false;
            float time = 0f;
            var cameraStart = new Vector3(28.2f, 30.15f, 7.5f);
            var look = new Vector3(30.7f, 29.15f, 9.3f);
            Vector3 standing = new Vector3(31.72f, 28.1f, 8.05f);
            Quaternion standingRotation = Quaternion.Euler(0f, 180f, 0f);
            while (time < wakeSeconds)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / wakeSeconds);
                float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .75f));
                sleepingPlayer.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, standing, rise), Quaternion.Slerp(startRotation, standingRotation, rise));
                cameraTransform.position = cameraStart;
                cameraTransform.rotation = Quaternion.LookRotation(look - cameraStart);
                if (t > .70f)
                {
                    float blend = Mathf.SmoothStep(0f, 1f, (t - .70f) / .30f);
                    Vector3 endPos = cameraParent.TransformPoint(cameraLocalPosition);
                    Quaternion endRot = cameraParent.rotation * cameraLocalRotation;
                    cameraTransform.SetPositionAndRotation(Vector3.Lerp(cameraStart, endPos, blend), Quaternion.Slerp(cameraTransform.rotation, endRot, blend));
                }
                yield return null;
            }
            Complete();
        }

        private IEnumerator Shot(Vector3 position, Vector3 target, float seconds, bool vibrate)
        {
            float time = 0f;
            while (time < seconds)
            {
                time += Time.deltaTime;
                cameraTransform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
                if (vibrate && phone != null)
                    phone.localPosition = phonePosition + new Vector3(Mathf.Sin(time * 65f) * .0015f, 0f, Mathf.Sin(time * 53f) * .001f);
                yield return null;
            }
        }

        private void CreateAlarm()
        {
            const int rate = 22050; const float duration = 2f;
            float[] samples = new float[(int)(rate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate, phase = t % .5f;
                float envelope = Mathf.Clamp01(phase / .015f) * Mathf.Clamp01((.32f - phase) / .035f);
                float frequency = ((int)(t / .5f) % 2 == 0) ? 880f : 660f;
                samples[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) * .23f + Mathf.Sin(2f * Mathf.PI * frequency * 2f * t) * .06f) * envelope;
            }
            alarmClip = AudioClip.Create("OndaS - alerta sísmica", samples.Length, 1, rate, false); alarmClip.SetData(samples, 0);
            alarm = phone.gameObject.AddComponent<AudioSource>(); alarm.clip = alarmClip; alarm.loop = true; alarm.playOnAwake = false; alarm.spatialBlend = 0f; alarm.volume = .7f;
            var lightObject = new GameObject("Luz de pantalla"); lightObject.transform.SetParent(phone, false);
            screenLight = lightObject.AddComponent<Light>(); screenLight.type = LightType.Point; screenLight.range = 1f; screenLight.intensity = .7f;
            screenLight.color = new Color(1f, .65f, .3f); screenLight.enabled = false;
        }

        private static Transform FindChild(Transform root, string name)
        { foreach (var child in root.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child; return null; }

        private void Complete()
        {
            if (completed) return;
            completed = true;
            if (phone != null) phone.localPosition = phonePosition;
            if (screen != null) screen.gameObject.SetActive(true);
            if (cameraTransform != null && cameraParent != null) { cameraTransform.localPosition = cameraLocalPosition; cameraTransform.localRotation = cameraLocalRotation; }
            if (sleepingPlayer != null) Destroy(sleepingPlayer);
            if (alarm != null) Destroy(alarm);
            if (alarmClip != null) Destroy(alarmClip);
            if (screenLight != null) Destroy(screenLight.gameObject);
            if (bedLight != null) Destroy(bedLight.gameObject);
            if (staminaUi != null) staminaUi.SetActive(true);
            if (inventoryUi != null) inventoryUi.SetActive(true);
            if (earthquake != null) earthquake.SetIntroHudVisible(true);
            IsPlaying = false;
        }
        private void OnDestroy() { Complete(); }
    }
}
