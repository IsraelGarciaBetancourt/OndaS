using System;
using System.Collections;
using System.Collections.Generic;
using OndaS.Gameplay.Earthquake;
using OndaS.Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using OndaS.Visual;

namespace OndaS.Gameplay.Electrical
{
    public sealed class OndaSFuseBox : MonoBehaviour
    {
        private readonly List<OndaSFuseSwitch> switches = new List<OndaSFuseSwitch>();
        private readonly List<Vector3> switchCenters = new List<Vector3>();
        private OndaSFirstEarthquake earthquake;
        private GameObject objectiveCanvas;
        private TextMeshProUGUI objectiveText;
        private float completedAt = -1f;
        private Transform doorPivot;
        private BoxCollider interactionCollider;
        private Camera inspectionCamera;
        private OndaSLookDevWalkthrough playerMovement;
        private Vector3 cameraLocalPosition;
        private Quaternion cameraLocalRotation;
        private float originalFieldOfView;
        private Coroutine inspectionTransition;
        private bool transitionInProgress;
        public bool IsInspecting { get; private set; }

        // Future electrical hazards can query or subscribe to this state.
        public bool IsPowerIsolated => switches.Count == 5 && switches.TrueForAll(item => !item.IsOn);
        public event Action<bool> PowerIsolationChanged;

        private void Awake()
        {
            earthquake = FindFirstObjectByType<OndaSFirstEarthquake>();
            SetupDoorAndBacking();
            for (int i = 1; i <= 5; i++)
            {
                var part = transform.Find("Switch_0" + i);
                if (part == null) { Debug.LogWarning("Caja de luz: falta Switch_0" + i, this); continue; }
                var filter = part.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                switchCenters.Add(transform.InverseTransformPoint(part.TransformPoint(filter.sharedMesh.bounds.center)));
                var pivot = new GameObject("OndaS - interruptor " + i);
                pivot.transform.SetParent(transform, false);
                pivot.transform.position = part.TransformPoint(filter.sharedMesh.bounds.center);
                pivot.transform.rotation = part.rotation;
                part.SetParent(pivot.transform, true);
                var hit = part.gameObject.AddComponent<BoxCollider>();
                var bounds = filter.sharedMesh.bounds;
                hit.center = bounds.center;
                hit.size = new Vector3(Mathf.Max(bounds.size.x, 0.16f), Mathf.Max(bounds.size.y, 0.27f), Mathf.Max(bounds.size.z, 0.23f));
                var toggle = pivot.AddComponent<OndaSFuseSwitch>();
                toggle.Initialize(this);
                switches.Add(toggle);
            }
            CreateObjective();
        }

        private void Update()
        {
            if (objectiveCanvas == null) return;
            bool show = earthquake != null && earthquake.HasFinished && (!IsPowerIsolated || Time.time < completedAt + 3f);
            objectiveCanvas.SetActive(show);
            if (!show) return;
            int off = switches.FindAll(item => !item.IsOn).Count;
            objectiveText.text = IsPowerIsolated ? "CORRIENTE CORTADA" : "BAJA LOS INTERRUPTORES  " + off + "/5";
        }

        internal void NotifySwitchChanged()
        {
            completedAt = IsPowerIsolated ? Time.time : -1f;
            PowerIsolationChanged?.Invoke(IsPowerIsolated);
        }

        private void SetupDoorAndBacking()
        {
            var door = transform.Find("Puerta_Abierta");
            if (door != null)
            {
                var hinge = new GameObject("OndaS - Bisagra caja");
                doorPivot = hinge.transform;
                doorPivot.SetParent(transform, false);
                doorPivot.localPosition = new Vector3(-0.8f, 0f, 0.46f);
                door.SetParent(doorPivot, true);
                doorPivot.localRotation = Quaternion.Euler(0f, 120f, 0f);
            }
            var body = transform.Find("Caja_Metal_Cuerpo");
            var bodyRenderer = body != null ? body.GetComponent<Renderer>() : null;
            if (bodyRenderer != null)
            {
                var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                back.name = "OndaS - Fondo metálico";
                back.transform.SetParent(transform, false);
                back.transform.localPosition = new Vector3(0f, 0f, -0.465f);
                back.transform.localScale = new Vector3(1.52f, 1.85f, 0.035f);
                back.GetComponent<Renderer>().sharedMaterial = bodyRenderer.sharedMaterial;
                Destroy(back.GetComponent<Collider>());
            }
            var target = new GameObject("OndaS - Abrir caja");
            target.transform.SetParent(transform, false);
            interactionCollider = target.AddComponent<BoxCollider>();
            interactionCollider.center = new Vector3(0f, 0f, 0.57f);
            interactionCollider.size = new Vector3(1.7f, 2.05f, 0.18f);
        }

        public bool BeginInspection(Camera camera, OndaSLookDevWalkthrough movement)
        {
            if (IsInspecting || camera == null || movement == null || !movement.enabled) return false;
            inspectionCamera = camera;
            playerMovement = movement;
            cameraLocalPosition = camera.transform.localPosition;
            cameraLocalRotation = camera.transform.localRotation;
            originalFieldOfView = camera.fieldOfView;
            movement.enabled = false;
            IsInspecting = true;
            interactionCollider.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inspectionTransition = StartCoroutine(AnimateInspection(true));
            return true;
        }

        public void HandleInspectionInput()
        {
            if (!IsInspecting || transitionInProgress) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame && playerMovement != null)
                playerMovement.ToggleFlashlight();
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                inspectionTransition = StartCoroutine(AnimateInspection(false));
                return;
            }
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || inspectionCamera == null) return;
            if (TryPickSwitch(mouse.position.ReadValue(), out var selected)) selected.Toggle();
        }

        private bool TryPickSwitch(Vector2 screenPosition, out OndaSFuseSwitch selected)
        {
            selected = null;
            if (inspectionCamera == null || Screen.width <= 0 || Screen.height <= 0) return false;
            // The world camera renders to a low-resolution texture stretched across the screen.
            // Use normalized display coordinates rather than its render texture pixel dimensions.
            var viewport = new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);
            if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f) return false;
            Ray ray = inspectionCamera.ViewportPointToRay(viewport);
            Vector3 origin = transform.InverseTransformPoint(ray.origin);
            Vector3 direction = transform.InverseTransformDirection(ray.direction);
            if (Mathf.Abs(direction.z) < 0.0001f) return false;
            float travel = (0.15f - origin.z) / direction.z;
            if (travel < 0f) return false;
            Vector3 point = origin + direction * travel;
            float closestX = 0.09f;
            for (int i = 0; i < switches.Count; i++)
            {
                Vector3 center = switchCenters[i];
                float dx = Mathf.Abs(point.x - center.x);
                // Cover both positions and the gap traversed by the moving handle.
                if (dx > closestX || point.y < center.y - 0.27f || point.y > center.y + 0.21f) continue;
                closestX = dx;
                selected = switches[i];
            }
            return selected != null;
        }

        private IEnumerator AnimateInspection(bool opening)
        {
            transitionInProgress = true;
            Vector3 fromPosition = inspectionCamera.transform.position;
            Quaternion fromRotation = inspectionCamera.transform.rotation;
            Quaternion doorFrom = doorPivot != null ? doorPivot.localRotation : Quaternion.identity;
            Quaternion doorTo = opening ? Quaternion.identity : Quaternion.Euler(0f, 120f, 0f);
            float fromFieldOfView = inspectionCamera.fieldOfView;
            float elapsed = 0f;
            const float duration = 0.55f;
            while (elapsed < duration && inspectionCamera != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                var parent = inspectionCamera.transform.parent;
                Vector3 targetPosition = opening ? transform.TransformPoint(0f, 0f, 1.45f) : parent.TransformPoint(cameraLocalPosition);
                Quaternion targetRotation = opening ? Quaternion.LookRotation(transform.TransformPoint(0f, 0f, 0f) - targetPosition, transform.up) : parent.rotation * cameraLocalRotation;
                inspectionCamera.transform.SetPositionAndRotation(Vector3.Lerp(fromPosition, targetPosition, t), Quaternion.Slerp(fromRotation, targetRotation, t));
                inspectionCamera.fieldOfView = Mathf.Lerp(fromFieldOfView, opening ? 75f : originalFieldOfView, t);
                if (doorPivot != null) doorPivot.localRotation = Quaternion.Slerp(doorFrom, doorTo, t);
                yield return null;
            }
            if (doorPivot != null) doorPivot.localRotation = doorTo;
            transitionInProgress = false;
            inspectionTransition = null;
            if (!opening) RestorePlayer();
        }

        public void InterruptInspection()
        {
            if (!IsInspecting) return;
            if (inspectionTransition != null) StopCoroutine(inspectionTransition);
            inspectionTransition = null;
            transitionInProgress = false;
            if (doorPivot != null) doorPivot.localRotation = Quaternion.Euler(0f, 120f, 0f);
            RestorePlayer();
        }

        private void RestorePlayer()
        {
            if (inspectionCamera != null)
            {
                inspectionCamera.transform.localPosition = cameraLocalPosition;
                inspectionCamera.transform.localRotation = cameraLocalRotation;
                inspectionCamera.fieldOfView = originalFieldOfView;
            }
            if (playerMovement != null) playerMovement.enabled = true;
            if (interactionCollider != null) interactionCollider.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inspectionCamera = null;
            playerMovement = null;
            IsInspecting = false;
        }

        private void CreateObjective()
        {
            objectiveCanvas = new GameObject("OndaS - Objetivo eléctrico", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = objectiveCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = objectiveCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            var label = new GameObject("Objetivo", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(objectiveCanvas.transform, false);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(50f, -35f); rect.sizeDelta = new Vector2(800f, 55f);
            objectiveText = label.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(objectiveText); objectiveText.fontSize = 28f;
            objectiveText.alignment = TextAlignmentOptions.TopLeft; objectiveText.raycastTarget = false;
            objectiveCanvas.SetActive(false);
        }

        private void OnDestroy()
        {
            InterruptInspection();
            if (objectiveCanvas != null) Destroy(objectiveCanvas);
        }
    }

    public sealed class OndaSFuseSwitch : MonoBehaviour
    {
        private OndaSFuseBox box;
        private Quaternion baseRotation;
        private Vector3 basePosition;
        public bool IsOn { get; private set; } = true;

        internal void Initialize(OndaSFuseBox owner)
        {
            box = owner;
            baseRotation = transform.localRotation;
            basePosition = transform.localPosition;
            transform.localRotation = TargetRotation;
            transform.localPosition = TargetPosition;
        }

        private Quaternion TargetRotation => baseRotation * Quaternion.AngleAxis(IsOn ? -35f : 48f, Vector3.right);
        private Vector3 TargetPosition => basePosition + Vector3.up * (IsOn ? 0.06f : -0.12f);

        private void Update()
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, TargetRotation, 1f - Mathf.Exp(-12f * Time.deltaTime));
            transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }

        public void Toggle()
        {
            if (box == null) return;
            IsOn = !IsOn;
            box.NotifySwitchChanged();
        }
    }
}
