using UnityEngine;
using UnityEngine.UI;

namespace OndaS.Visual
{
    // Presents the world at a fixed vertical resolution while leaving overlay UI sharp.
    [RequireComponent(typeof(Camera))]
    public sealed class OndaSRetroCamera : MonoBehaviour
    {
        [SerializeField, Range(180, 720)] private int verticalResolution = 360;
        [SerializeField] private Material compositeMaterial;

        private Camera worldCamera;
        private RenderTexture worldTexture;
        private RenderTexture previousTarget;
        private GameObject displayObject;
        private GameObject displayCameraObject;
        private RawImage display;
        private int lastWidth;
        private int lastHeight;

        private void OnEnable()
        {
            worldCamera = GetComponent<Camera>();
            previousTarget = worldCamera.targetTexture;
            // The world camera renders to a texture, so Unity still needs a camera
            // targeting the display to present the overlay without an editor warning.
            displayCameraObject = new GameObject("OndaS Display Camera", typeof(Camera));
            var displayCamera = displayCameraObject.GetComponent<Camera>();
            displayCamera.cullingMask = 0;
            displayCamera.clearFlags = CameraClearFlags.SolidColor;
            displayCamera.backgroundColor = Color.black;
            displayCamera.depth = worldCamera.depth - 1;
            displayCamera.targetDisplay = worldCamera.targetDisplay;
            displayCamera.allowMSAA = false;
            displayCamera.allowHDR = false;
            displayObject = new GameObject("OndaS Retro Display", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = displayObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -100;
            canvas.targetDisplay = worldCamera.targetDisplay;
            var imageObject = new GameObject("World", typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(displayObject.transform, false);
            display = imageObject.GetComponent<RawImage>();
            display.raycastTarget = false;
            display.material = compositeMaterial;
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Resize();
        }

        private void Update()
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height ||
                (worldTexture != null && worldTexture.height != verticalResolution)) Resize();
        }

        private void Resize()
        {
            // The Game view can briefly report a 1-pixel dimension during Play
            // transitions or while its panel is collapsed. Wait for a usable size.
            if (Screen.width < 32 || Screen.height < 32) return;
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            int height = Mathf.Max(180, verticalResolution);
            int width = Mathf.Clamp(Mathf.RoundToInt(height * ((float)lastWidth / lastHeight)), 1, SystemInfo.maxTextureSize);
            worldCamera.targetTexture = previousTarget;
            if (worldTexture != null) { worldTexture.Release(); Destroy(worldTexture); }
            worldTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "OndaS Retro World",
                filterMode = FilterMode.Point,
                antiAliasing = 1,
                useMipMap = false
            };
            worldTexture.Create();
            worldCamera.targetTexture = worldTexture;
            display.texture = worldTexture;
        }

        private void OnDisable()
        {
            if (worldCamera != null) worldCamera.targetTexture = previousTarget;
            if (worldTexture != null) { worldTexture.Release(); Destroy(worldTexture); worldTexture = null; }
            if (displayObject != null) Destroy(displayObject);
            if (displayCameraObject != null) Destroy(displayCameraObject);
        }
    }
}
