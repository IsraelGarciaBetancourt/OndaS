using OndaS.Visual;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static OndaS.Gameplay.UI.OndaSMenuUi;

namespace OndaS.Gameplay.UI
{
    [DefaultExecutionOrder(-50)]
    internal sealed class OndaSPauseMenu : MonoBehaviour
    {
        private OndaSLookDevWalkthrough movement;
        private OndaSDoorInteractor interactor;
        private GameObject overlay;
        private Button resumeButton;
        private GameObject previousSelection;
        private bool isOpen;
        private int closedFrame = -1;
        private float previousTimeScale;
        private bool previousAudioPause;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisibility;

        public bool BlocksGameplay => isOpen || Time.frameCount == closedFrame;

        private void Awake()
        {
            movement = GetComponent<OndaSLookDevWalkthrough>();
            interactor = GetComponent<OndaSDoorInteractor>();
        }

        private void Update()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame != true) return;
            if (isOpen)
            {
                Close();
                return;
            }

            bool canPause = interactor != null ? interactor.isActiveAndEnabled : movement.isActiveAndEnabled;
            if (canPause && !OndaSIntroSequence.IsPlaying && Time.timeScale > 0f)
                Open();
        }

        private void Open()
        {
            if (overlay == null) BuildOverlay();
            previousSelection = EventSystem.current?.currentSelectedGameObject;
            previousTimeScale = Time.timeScale;
            previousAudioPause = AudioListener.pause;
            previousCursorLock = movement.isActiveAndEnabled ? CursorLockMode.Locked : Cursor.lockState;
            previousCursorVisibility = !movement.isActiveAndEnabled && Cursor.visible;

            isOpen = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            overlay.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);
        }

        private void Close(bool restoreCursor = true)
        {
            if (!isOpen) return;
            isOpen = false;
            closedFrame = Time.frameCount;
            var events = EventSystem.current;
            if (events != null)
                events.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy
                    ? previousSelection : null);
            previousSelection = null;
            if (overlay != null) overlay.SetActive(false);
            Time.timeScale = previousTimeScale;
            AudioListener.pause = previousAudioPause;
            if (restoreCursor)
            {
                Cursor.lockState = previousCursorLock;
                Cursor.visible = previousCursorVisibility;
            }
        }

        private void ReturnToMainMenu()
        {
            Close(restoreCursor: false);
            SceneManager.LoadScene(OndaSMainMenu.ScenePath);
        }

        private void OnDisable() => Close();

        private void OnDestroy()
        {
            if (overlay != null) Destroy(overlay);
        }

        private void BuildOverlay()
        {
            overlay = new GameObject("Menú de pausa", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.SetActive(false);
            var root = overlay.GetComponent<RectTransform>();
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            if (EventSystem.current == null)
            {
                var events = new GameObject("Eventos de pausa");
                events.transform.SetParent(root, false);
                events.AddComponent<EventSystem>();
                events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            Image(root, "Sombra", Vector2.zero, Vector2.zero, new Color(0, 0, 0, .72f), true);
            var panel = Image(root, "Panel", Vector2.zero, new Vector2(710, 540),
                new Color32(24, 20, 19, 255)).rectTransform;
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
            panel.anchoredPosition = Vector2.zero;

            Text(panel, "Título", "PAUSA", new Vector2(52, -32), new Vector2(600, 74), 64, Paper);
            Image(panel, "Acento", new Vector2(54, -118), new Vector2(74, 4), Red).raycastTarget = false;
            Text(panel, "Volumen", "VOLUMEN GENERAL", new Vector2(54, -146), new Vector2(600, 46), 34, Paper);
            AddVolumeSlider(panel, new Vector2(54, -197));
            Text(panel, "Ratón", "SENSIBILIDAD DEL RATÓN", new Vector2(54, -271), new Vector2(600, 46), 34, Paper);
            AddSensitivitySlider(panel, new Vector2(54, -322), value => movement.SetMouseSensitivity(value));

            resumeButton = AddButton(panel, "REANUDAR", 54, new Color32(117, 41, 34, 255), () => Close());
            AddButton(panel, "MENÚ PRINCIPAL", 362, new Color32(57, 47, 44, 255), ReturnToMainMenu);
            Text(panel, "Atajo", "ESC PARA REANUDAR", new Vector2(56, -486), new Vector2(590, 28), 22,
                new Color32(176, 165, 149, 255));
        }

        private static Button AddButton(RectTransform parent, string label, float x, Color color, UnityAction action)
        {
            var image = Image(parent, label, new Vector2(x, -409), new Vector2(292, 62), color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color32(255, 223, 198, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color32(212, 174, 149, 255);
            button.colors = colors;
            var text = Text(image.rectTransform, "Etiqueta", label, Vector2.zero, image.rectTransform.sizeDelta, 32, Paper);
            text.alignment = TextAlignmentOptions.Center;
            button.onClick.AddListener(action);
            return button;
        }
    }
}
