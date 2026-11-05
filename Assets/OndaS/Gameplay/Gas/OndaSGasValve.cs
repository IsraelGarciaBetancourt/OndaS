using System;
using System.Collections;
using OndaS.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OndaS.Gameplay.Gas
{
    public sealed class OndaSGasValve : MonoBehaviour
    {
        private Transform valvePivot;
        private Quaternion valveRest;
        private Camera inspectionCamera;
        private OndaSLookDevWalkthrough movement;
        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private float originalFieldOfView;
        private Vector3 closeupPosition;
        private Coroutine transition;
        private bool transitioning;

        public bool IsInspecting { get; private set; }
        public bool IsClosed { get; private set; }
        public event Action<bool> ValveClosedChanged;

        private void Awake()
        {
            var body = transform.Find("Cylinder_1_Object_8");
            var filter = body != null ? body.GetComponent<MeshFilter>() : null;
            if (filter != null && filter.sharedMesh != null && body.GetComponent<Collider>() == null)
            {
                var collider = body.gameObject.AddComponent<BoxCollider>();
                collider.center = filter.sharedMesh.bounds.center;
                collider.size = filter.sharedMesh.bounds.size;
            }
            var valve = transform.Find("Cap_0_Object_6");
            var renderer = valve != null ? valve.GetComponent<Renderer>() : null;
            if (renderer == null) { Debug.LogWarning("Balón de gas: no se encontró la válvula.", this); enabled = false; return; }
            valvePivot = new GameObject("OndaS - Pivote válvula").transform;
            valvePivot.SetParent(transform, false);
            valvePivot.position = renderer.bounds.center;
            valvePivot.rotation = transform.rotation;
            valve.SetParent(valvePivot, true);
            valveRest = valvePivot.localRotation;
        }

        private void Update()
        {
            if (valvePivot != null)
            {
                var target = valveRest * Quaternion.AngleAxis(IsClosed ? -180f : 0f, Vector3.up);
                valvePivot.localRotation = Quaternion.Slerp(valvePivot.localRotation, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
            }
        }

        public bool BeginInspection(Camera camera, OndaSLookDevWalkthrough player)
        {
            if (IsInspecting || valvePivot == null || camera == null || player == null || !player.enabled) return false;
            inspectionCamera = camera; movement = player;
            originalLocalPosition = camera.transform.localPosition;
            originalLocalRotation = camera.transform.localRotation;
            originalFieldOfView = camera.fieldOfView;
            Vector3 away = Vector3.ProjectOnPlane(camera.transform.position - valvePivot.position, Vector3.up).normalized;
            if (away.sqrMagnitude < 0.01f) away = -transform.forward;
            closeupPosition = valvePivot.position + away * 0.45f + Vector3.up * 0.65f;
            movement.enabled = false;
            IsInspecting = true;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            transition = StartCoroutine(AnimateInspection(true));
            return true;
        }

        public void HandleInspectionInput()
        {
            if (!IsInspecting || transitioning) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame && movement != null) movement.ToggleFlashlight();
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            { transition = StartCoroutine(AnimateInspection(false)); return; }
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || inspectionCamera == null) return;
            Vector2 point = mouse.position.ReadValue();
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Vector2 viewport = new Vector2(point.x / Screen.width, point.y / Screen.height);
            if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f) return;
            Ray ray = inspectionCamera.ViewportPointToRay(viewport);
            Vector3 toValve = valvePivot.position - ray.origin;
            float along = Vector3.Dot(toValve, ray.direction);
            float radius = 0.075f * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            if (along > 0f && (toValve - ray.direction * along).sqrMagnitude <= radius * radius)
            {
                IsClosed = !IsClosed;
                ValveClosedChanged?.Invoke(IsClosed);
            }
        }

        private IEnumerator AnimateInspection(bool opening)
        {
            transitioning = true;
            Vector3 start = inspectionCamera.transform.position;
            Quaternion rotation = inspectionCamera.transform.rotation;
            float fov = inspectionCamera.fieldOfView;
            float elapsed = 0f;
            while (elapsed < 0.55f && inspectionCamera != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.55f));
                var parent = inspectionCamera.transform.parent;
                Vector3 destination = opening ? closeupPosition : parent.TransformPoint(originalLocalPosition);
                Quaternion target = opening ? Quaternion.LookRotation(valvePivot.position - destination, Vector3.up) : parent.rotation * originalLocalRotation;
                inspectionCamera.transform.SetPositionAndRotation(Vector3.Lerp(start, destination, t), Quaternion.Slerp(rotation, target, t));
                inspectionCamera.fieldOfView = Mathf.Lerp(fov, opening ? 48f : originalFieldOfView, t);
                yield return null;
            }
            transition = null; transitioning = false;
            if (!opening) RestorePlayer();
        }

        public void InterruptInspection()
        {
            if (!IsInspecting) return;
            if (transition != null) StopCoroutine(transition);
            transition = null; transitioning = false;
            RestorePlayer();
        }

        private void RestorePlayer()
        {
            if (inspectionCamera != null)
            {
                inspectionCamera.transform.localPosition = originalLocalPosition;
                inspectionCamera.transform.localRotation = originalLocalRotation;
                inspectionCamera.fieldOfView = originalFieldOfView;
            }
            if (movement != null) movement.enabled = true;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            inspectionCamera = null; movement = null; IsInspecting = false;
        }

        private void OnDestroy() => InterruptInspection();
    }
}
