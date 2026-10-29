using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace OndaS.Visual
{
    // Temporary navigation and lighting controls for the look-development scene.
    [RequireComponent(typeof(CharacterController))]
    public sealed class OndaSLookDevWalkthrough : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private float mouseSensitivity = 0.10f;
        private CharacterController controller;
        private Light torch;
        private Light[] sceneLights;
        private bool[] lightStates;
        private float verticalSpeed;
        private float pitch;
        private bool blackout;
        private AmbientMode ambientMode;
        private Color ambientSky, ambientEquator, ambientGround, ambientLight;
        private float ambientIntensity;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            sceneLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            lightStates = new bool[sceneLights.Length];
            var lightObject = new GameObject("OndaS - Linterna de prueba");
            lightObject.transform.SetParent(viewCamera.transform, false);
            torch = lightObject.AddComponent<Light>();
            torch.type = LightType.Spot;
            torch.range = 14f;
            torch.spotAngle = 65f;
            torch.innerSpotAngle = 38f;
            torch.intensity = 4f;
            torch.color = new Color(1f, 0.91f, 0.73f);
            torch.shadows = LightShadows.Soft;
            torch.enabled = false;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
                viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                transform.Rotate(Vector3.up * delta.x);
            }
            Vector2 input = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                                       (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            input = Vector2.ClampMagnitude(input, 1f);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            verticalSpeed += -15f * Time.deltaTime;
            controller.Move(((transform.right * input.x + transform.forward * input.y) * speed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (keyboard.bKey.wasPressedThisFrame) SetBlackout(!blackout);
            if (keyboard.fKey.wasPressedThisFrame) torch.enabled = !torch.enabled;
        }

        public void ToggleBlackoutPreview() { SetBlackout(!blackout); }

        private void SetBlackout(bool value)
        {
            blackout = value;
            if (value)
            {
                ambientMode = RenderSettings.ambientMode; ambientSky = RenderSettings.ambientSkyColor;
                ambientEquator = RenderSettings.ambientEquatorColor; ambientGround = RenderSettings.ambientGroundColor;
                ambientLight = RenderSettings.ambientLight; ambientIntensity = RenderSettings.ambientIntensity;
                for (int i = 0; i < sceneLights.Length; i++) if (sceneLights[i] != null)
                { lightStates[i] = sceneLights[i].enabled; sceneLights[i].enabled = false; }
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.025f, 0.03f, 0.035f);
                RenderSettings.ambientIntensity = 0.1f;
                torch.enabled = true;
            }
            else
            {
                for (int i = 0; i < sceneLights.Length; i++) if (sceneLights[i] != null) sceneLights[i].enabled = lightStates[i];
                RenderSettings.ambientMode = ambientMode; RenderSettings.ambientSkyColor = ambientSky;
                RenderSettings.ambientEquatorColor = ambientEquator; RenderSettings.ambientGroundColor = ambientGround;
                RenderSettings.ambientLight = ambientLight; RenderSettings.ambientIntensity = ambientIntensity;
                torch.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (blackout) SetBlackout(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
