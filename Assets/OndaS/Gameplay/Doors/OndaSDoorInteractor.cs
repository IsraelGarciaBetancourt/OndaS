using UnityEngine;
using UnityEngine.InputSystem;

namespace OndaS.Gameplay
{
    public sealed class OndaSDoorInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        private OndaSDoor focusedDoor;
        private GUIStyle promptStyle;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            focusedDoor = null;
            if (viewCamera == null || Cursor.lockState != CursorLockMode.Locked) return;
            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out var hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore)) return;
            focusedDoor = hit.collider.GetComponentInParent<OndaSDoor>();
            var keyboard = Keyboard.current;
            if (focusedDoor != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                focusedDoor.Toggle();
        }

        private void OnGUI()
        {
            if (focusedDoor == null || !focusedDoor.CanInteract) return;
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18, fontStyle = FontStyle.Bold };
                promptStyle.normal.textColor = Color.white;
            }
            var label = focusedDoor.IsOpen ? "[E] Cerrar puerta" : "[E] Abrir puerta";
            var rect = new Rect(Screen.width * 0.5f - 120, Screen.height * 0.5f + 28, 240, 34);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(rect, label, promptStyle);
        }
    }
}
