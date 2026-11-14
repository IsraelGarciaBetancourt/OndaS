using OndaS.Gameplay.UI;
using OndaS.Visual;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OndaS.Gameplay
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class OndaSVictoryZone : MonoBehaviour
    {
        private bool won;
        private float previousTimeScale;
        private GameObject victoryCanvas;
        private BoxCollider zone;
        private CharacterController playerController;

        private void Reset() => ConfigureTrigger();
        private void Awake()
        {
            ConfigureTrigger();
            var player = FindAnyObjectByType<OndaSLookDevWalkthrough>();
            if (player != null) playerController = player.GetComponent<CharacterController>();
        }

        private void ConfigureTrigger()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other) => TryWin(other);
        private void OnTriggerStay(Collider other) => TryWin(other);

        private void TryWin(Collider other)
        {
            if (won || !(other is CharacterController)) return;
            var player = other.GetComponent<OndaSLookDevWalkthrough>();
            if (player == null || !player.isActiveAndEnabled) return;

            won = true;
            previousTimeScale = Time.timeScale;
            var interactor = player.GetComponent<OndaSDoorInteractor>();
            if (interactor != null) interactor.enabled = false;
            player.enabled = false;
            zone.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowVictory();
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (!won)
            {
                if (playerController != null && playerController.enabled && zone.enabled &&
                    Physics.ComputePenetration(zone, zone.transform.position, zone.transform.rotation,
                        playerController, playerController.transform.position, playerController.transform.rotation,
                        out _, out _))
                    TryWin(playerController);
                return;
            }
            var keyboard = Keyboard.current;
            if (keyboard == null || (!keyboard.rKey.wasPressedThisFrame && !keyboard.enterKey.wasPressedThisFrame && !keyboard.numpadEnterKey.wasPressedThisFrame)) return;
            Time.timeScale = previousTimeScale;
            SceneManager.LoadScene(gameObject.scene.path);
        }

        private void ShowVictory()
        {
            victoryCanvas = new GameObject("OndaS - Victoria", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = victoryCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = victoryCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var background = new GameObject("Fondo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(victoryCanvas.transform, false);
            var rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.015f, 0.015f, 0.012f, 0.98f);
            CreateText("Título", "¡ESCAPASTE!", new Vector2(0f, 80f), 72f, new Color32(255, 218, 65, 255));
            CreateText("Mensaje", "Has salido del edificio", new Vector2(0f, -15f), 32f, new Color32(230, 220, 200, 255));
            CreateText("Reiniciar", "[R / ENTER] VOLVER A JUGAR", new Vector2(0f, -115f), 34f, new Color32(255, 218, 65, 255));
        }

        private void CreateText(string name, string value, Vector2 position, float size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            obj.transform.SetParent(victoryCanvas.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(1200f, 100f);
            var text = obj.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(text);
            text.text = value; text.fontSize = size; text.color = color;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (won) Time.timeScale = previousTimeScale;
            if (victoryCanvas != null) Destroy(victoryCanvas);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.15f);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.9f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
