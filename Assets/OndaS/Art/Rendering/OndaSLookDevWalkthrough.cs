using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using OndaS.Gameplay.UI;
using OndaS.Gameplay.Items;

namespace OndaS.Visual
{
    // Temporary navigation and lighting controls for the look-development scene.
    [RequireComponent(typeof(CharacterController))]
    public sealed class OndaSLookDevWalkthrough : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private float sprintSpeed = 5.5f;
        [SerializeField] private float crouchSpeed = 1.8f;
        [SerializeField, Range(0.4f, 0.95f)] private float crouchHeightRatio = 0.6f;
        [SerializeField, Min(0.1f)] private float postureSpeed = 4f;
        [SerializeField] private float mouseSensitivity = 0.10f;
        [Header("Stamina")]
        [SerializeField, Min(1f)] private float maxStamina = 100f;
        [SerializeField, Min(0.01f)] private float sprintDrainPerSecond = 20f;
        [SerializeField, Min(0.01f)] private float staminaRecoveryPerSecond = 15f;
        [SerializeField, Min(0f)] private float staminaRecoveryDelay = 1f;
        [SerializeField, Range(0.01f, 1f)] private float exhaustedRecoveryFraction = 0.25f;
        private CharacterController controller;
        private Light torch;
        private Light nightFill;
        private Light[] sceneLights;
        private bool[] lightStates;
        private float[] sceneLightIntensities;
        private OndaSRetroCamera retroCamera;
        private float verticalSpeed;
        private float pitch;
        private bool blackout;
        private bool seismicBlackout, nightApplied;
        private AmbientMode ambientMode;
        private Color ambientSky, ambientEquator, ambientGround, ambientLight;
        private float ambientIntensity;
        private AmbientMode originalAmbientMode;
        private Color originalAmbientSky, originalAmbientEquator, originalAmbientGround, originalAmbientLight, originalFogColor;
        private float originalAmbientIntensity, originalReflectionIntensity, blackoutReflectionIntensity;
        private Material originalSkybox;
        private CameraClearFlags originalClearFlags;
        private Color originalCameraBackground;
        private Light[] directionalLights;
        private bool[] directionalStates;
        private LightmapData[] originalLightmaps;
        private readonly List<Renderer> hiddenGlows = new List<Renderer>();
        private readonly List<Renderer> emissiveRenderers = new List<Renderer>();
        private readonly List<MaterialPropertyBlock> originalPropertyBlocks = new List<MaterialPropertyBlock>();
        private float standingHeight, standingBottom, standingCameraY;
        private bool crouchRequested;
        private float stamina;
        private float recoveryWait;
        private bool exhausted;
        private bool mustReleaseSprint;
        private OndaSStaminaHud staminaHud;
        private readonly Collider[] clearanceHits = new Collider[32];
        public bool IsSprinting { get; private set; }
        public bool IsCrouching => controller != null && controller.height < standingHeight - 0.01f;
        public float Stamina => stamina;
        public float MaxStamina => maxStamina;
        public bool IsExhausted => exhausted;
        public bool IsBlackout => blackout;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            stamina = maxStamina;
            var hudObject = new GameObject("Stamina UI", typeof(RectTransform));
            staminaHud = hudObject.AddComponent<OndaSStaminaHud>();
            staminaHud.Initialize(this);
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            standingHeight = controller.height;
            standingBottom = controller.center.y - standingHeight * 0.5f;
            standingCameraY = viewCamera.transform.localPosition.y;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            sceneLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            lightStates = new bool[sceneLights.Length];
            sceneLightIntensities = new float[sceneLights.Length];
            for (int i = 0; i < sceneLights.Length; i++) if (sceneLights[i] != null) sceneLightIntensities[i] = sceneLights[i].intensity;
            retroCamera = viewCamera.GetComponent<OndaSRetroCamera>();
            if (gameObject.scene.path == "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity") ApplyNight();
            var lightObject = new GameObject("OndaS - Linterna de prueba");
            lightObject.transform.SetParent(viewCamera.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, -0.02f, 0.22f);
            torch = lightObject.AddComponent<Light>();
            torch.type = LightType.Spot;
            torch.range = 18f;
            torch.spotAngle = 70f;
            torch.innerSpotAngle = 48f;
            torch.intensity = 14f;
            torch.color = new Color(1f, 0.91f, 0.73f);
            torch.shadows = LightShadows.Soft;
            torch.enabled = false;
            var fillObject = new GameObject("OndaS - Luz tenue nocturna");
            nightFill = fillObject.AddComponent<Light>();
            nightFill.type = LightType.Directional;
            nightFill.color = new Color(0.55f, 0.65f, 0.8f);
            nightFill.intensity = 0.16f;
            nightFill.shadows = LightShadows.None;
            fillObject.transform.rotation = Quaternion.Euler(45f, -25f, 0f);
            nightFill.enabled = false;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null)
            {
                UpdateStamina(false, false, Time.deltaTime);
                return;
            }
            if (keyboard.cKey.wasPressedThisFrame) crouchRequested = !crouchRequested;
            UpdatePosture();
            if (keyboard.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked && !OndaSBackpackRadial.IsOpen)
            {
                Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
                viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                transform.Rotate(Vector3.up * delta.x);
            }
            Vector2 input = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                                       (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            input = Vector2.ClampMagnitude(input, 1f);
            bool sprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            bool wantsSprint = !IsCrouching && !crouchRequested && input.sqrMagnitude > 0f && sprintHeld;
            UpdateStamina(wantsSprint, sprintHeld, Time.deltaTime);
            float moveSpeed = IsCrouching || crouchRequested ? crouchSpeed : IsSprinting ? sprintSpeed : speed;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            verticalSpeed += -15f * Time.deltaTime;
            controller.Move(((transform.right * input.x + transform.forward * input.y) * moveSpeed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (keyboard.bKey.wasPressedThisFrame && !seismicBlackout) SetBlackout(!blackout);
            if (keyboard.fKey.wasPressedThisFrame) torch.enabled = !torch.enabled;
        }

        public void ToggleBlackoutPreview() { if (!seismicBlackout) SetBlackout(!blackout); }

        public void TriggerSeismicBlackout()
        {
            seismicBlackout = true;
            SetSeismicFlicker(1f);
            SetBlackout(true);
            if (torch != null) torch.enabled = false;
        }

        public void SetSeismicFlicker(float brightness)
        {
            if (blackout) return;
            brightness = Mathf.Clamp01(brightness);
            for (int i = 0; i < sceneLights.Length; i++)
                if (sceneLights[i] != null) sceneLights[i].intensity = sceneLightIntensities[i] * brightness;
            if (retroCamera != null) retroCamera.SetPowerState(Mathf.Lerp(0.2f, 1f, brightness), false);
        }

        private void ApplyNight()
        {
            nightApplied = true;
            originalAmbientMode = RenderSettings.ambientMode;
            originalAmbientSky = RenderSettings.ambientSkyColor;
            originalAmbientEquator = RenderSettings.ambientEquatorColor;
            originalAmbientGround = RenderSettings.ambientGroundColor;
            originalAmbientLight = RenderSettings.ambientLight;
            originalAmbientIntensity = RenderSettings.ambientIntensity;
            originalReflectionIntensity = RenderSettings.reflectionIntensity;
            originalSkybox = RenderSettings.skybox;
            originalFogColor = RenderSettings.fogColor;
            originalClearFlags = viewCamera.clearFlags;
            originalCameraBackground = viewCamera.backgroundColor;
            var sunLights = new List<Light>();
            foreach (var light in sceneLights) if (light != null && light.type == LightType.Directional) sunLights.Add(light);
            directionalLights = sunLights.ToArray();
            directionalStates = new bool[directionalLights.Length];
            for (int i = 0; i < directionalLights.Length; i++)
            { directionalStates[i] = directionalLights[i].enabled; directionalLights[i].enabled = false; }
            RenderSettings.skybox = null;
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = new Color(0.008f, 0.012f, 0.025f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.055f, 0.065f, 0.09f);
            RenderSettings.ambientEquatorColor = new Color(0.026f, 0.031f, 0.042f);
            RenderSettings.ambientGroundColor = new Color(0.012f, 0.015f, 0.022f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.15f;
            RenderSettings.fogColor = new Color(0.025f, 0.03f, 0.045f);
        }

        private void RestoreNight()
        {
            if (!nightApplied) return;
            for (int i = 0; i < directionalLights.Length; i++)
                if (directionalLights[i] != null) directionalLights[i].enabled = directionalStates[i];
            RenderSettings.skybox = originalSkybox;
            RenderSettings.ambientMode = originalAmbientMode;
            RenderSettings.ambientSkyColor = originalAmbientSky;
            RenderSettings.ambientEquatorColor = originalAmbientEquator;
            RenderSettings.ambientGroundColor = originalAmbientGround;
            RenderSettings.ambientLight = originalAmbientLight;
            RenderSettings.ambientIntensity = originalAmbientIntensity;
            RenderSettings.reflectionIntensity = originalReflectionIntensity;
            RenderSettings.fogColor = originalFogColor;
            if (viewCamera != null) { viewCamera.clearFlags = originalClearFlags; viewCamera.backgroundColor = originalCameraBackground; }
            nightApplied = false;
        }

        private void UpdateStamina(bool wantsSprint, bool sprintHeld, float deltaTime)
        {
            if (!sprintHeld) mustReleaseSprint = false;
            if (exhausted && !mustReleaseSprint && stamina >= maxStamina * exhaustedRecoveryFraction)
                exhausted = false;

            IsSprinting = wantsSprint && !exhausted && stamina > 0f;
            if (IsSprinting)
            {
                stamina = Mathf.Max(0f, stamina - sprintDrainPerSecond * deltaTime);
                recoveryWait = staminaRecoveryDelay;
                if (stamina <= 0f)
                {
                    exhausted = true;
                    mustReleaseSprint = true;
                    IsSprinting = false;
                }
            }
            else
            {
                float delay = Mathf.Min(recoveryWait, deltaTime);
                recoveryWait -= delay;
                stamina = Mathf.Min(maxStamina, stamina + staminaRecoveryPerSecond * (deltaTime - delay));
            }
        }

        private void UpdatePosture()
        {
            bool shouldCrouch = crouchRequested || !HasStandingClearance();
            float target = shouldCrouch ? Mathf.Max(controller.radius * 2f, standingHeight * crouchHeightRatio) : standingHeight;
            float height = Mathf.MoveTowards(controller.height, target, postureSpeed * Time.deltaTime);
            controller.height = height;
            Vector3 center = controller.center;
            center.y = standingBottom + height * 0.5f;
            controller.center = center;
            Vector3 cameraPosition = viewCamera.transform.localPosition;
            cameraPosition.y = standingCameraY - (standingHeight - height);
            viewCamera.transform.localPosition = cameraPosition;
        }

        private bool HasStandingClearance()
        {
            if (controller.height >= standingHeight - 0.01f) return true;
            float radius = Mathf.Max(0.01f, controller.radius - 0.02f);
            Vector3 bottom = transform.TransformPoint(new Vector3(controller.center.x, standingBottom + radius + 0.02f, controller.center.z));
            Vector3 top = transform.TransformPoint(new Vector3(controller.center.x, standingBottom + standingHeight - radius - 0.02f, controller.center.z));
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, clearanceHits, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = clearanceHits[i];
                if (hit != null && !hit.transform.IsChildOf(transform) && !Physics.GetIgnoreLayerCollision(gameObject.layer, hit.gameObject.layer)) return false;
            }
            return count < clearanceHits.Length;
        }

        private void SetBlackout(bool value)
        {
            if (blackout == value) return;
            blackout = value;
            if (value)
            {
                ambientMode = RenderSettings.ambientMode; ambientSky = RenderSettings.ambientSkyColor;
                ambientEquator = RenderSettings.ambientEquatorColor; ambientGround = RenderSettings.ambientGroundColor;
                ambientLight = RenderSettings.ambientLight; ambientIntensity = RenderSettings.ambientIntensity;
                for (int i = 0; i < sceneLights.Length; i++) if (sceneLights[i] != null)
                { lightStates[i] = sceneLights[i].enabled; sceneLights[i].enabled = false; }
                originalLightmaps = LightmapSettings.lightmaps;
                LightmapSettings.lightmaps = System.Array.Empty<LightmapData>();
                blackoutReflectionIntensity = RenderSettings.reflectionIntensity;
                RenderSettings.reflectionIntensity = 0f;
                DisableEmissiveSurfaces();
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.018f, 0.021f, 0.028f);
                RenderSettings.ambientIntensity = 1f;
                if (nightFill != null) nightFill.enabled = true;
                if (retroCamera != null) retroCamera.SetPowerState(1f, true);
            }
            else
            {
                LightmapSettings.lightmaps = originalLightmaps;
                if (nightFill != null) nightFill.enabled = false;
                RenderSettings.reflectionIntensity = blackoutReflectionIntensity;
                RestoreEmissiveSurfaces();
                for (int i = 0; i < sceneLights.Length; i++) if (sceneLights[i] != null) sceneLights[i].enabled = lightStates[i];
                RenderSettings.ambientMode = ambientMode; RenderSettings.ambientSkyColor = ambientSky;
                RenderSettings.ambientEquatorColor = ambientEquator; RenderSettings.ambientGroundColor = ambientGround;
                RenderSettings.ambientLight = ambientLight; RenderSettings.ambientIntensity = ambientIntensity;
                if (retroCamera != null) retroCamera.SetPowerState(1f, false);
            }
        }

        private void DisableEmissiveSurfaces()
        {
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!renderer.gameObject.scene.IsValid() || renderer.transform.IsChildOf(transform)) continue;
                if (renderer.name.StartsWith("Glow", System.StringComparison.OrdinalIgnoreCase) ||
                    renderer.name.StartsWith("Flame", System.StringComparison.OrdinalIgnoreCase) ||
                    renderer.name.Equals("Bulb", System.StringComparison.OrdinalIgnoreCase) ||
                    renderer.name.StartsWith("Bulb.", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (renderer.enabled) { hiddenGlows.Add(renderer); renderer.enabled = false; }
                    continue;
                }
                bool hasEmission = false;
                foreach (var material in renderer.sharedMaterials)
                    if (material != null && material.HasProperty("_EmissionColor") && material.IsKeywordEnabled("_EMISSION"))
                    { hasEmission = true; break; }
                if (!hasEmission) continue;
                var previous = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(previous);
                var darkened = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(darkened);
                darkened.SetColor("_EmissionColor", Color.black);
                renderer.SetPropertyBlock(darkened);
                emissiveRenderers.Add(renderer);
                originalPropertyBlocks.Add(previous);
            }
        }

        private void RestoreEmissiveSurfaces()
        {
            foreach (var renderer in hiddenGlows) if (renderer != null) renderer.enabled = true;
            for (int i = 0; i < emissiveRenderers.Count; i++)
                if (emissiveRenderers[i] != null) emissiveRenderers[i].SetPropertyBlock(originalPropertyBlocks[i]);
            hiddenGlows.Clear(); emissiveRenderers.Clear(); originalPropertyBlocks.Clear();
        }

        private void OnDestroy()
        {
            if (staminaHud != null) Destroy(staminaHud.gameObject);
            if (blackout) SetBlackout(false);
            RestoreNight();
            if (nightFill != null) Destroy(nightFill.gameObject);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnEnable()
        {
            if (staminaHud != null) staminaHud.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            IsSprinting = false;
            if (staminaHud != null) staminaHud.gameObject.SetActive(false);
        }
    }
}
