using UnityEngine;

namespace OndaS.Gameplay
{
    public sealed class OndaSFirstPersonHands : MonoBehaviour
    {
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private CharacterController controller;
        [SerializeField] private float bobFrequency = 9f;
        [SerializeField] private float bobHeight = 0.035f;
        [SerializeField, Min(0.1f)] private float hiddenDrop = 0.5f;
        [SerializeField, Min(0.1f)] private float transitionSpeed = 8f;
        [SerializeField, Min(0f)] private float swingDegrees = 7f;
        private Vector3 leftRest, rightRest;
        private Quaternion leftRotation, rightRotation;
        private float phase, leftLift, rightLift;
        private Renderer leftRenderer, rightRenderer;

        private void Awake()
        {
            if (controller == null) controller = GetComponentInParent<CharacterController>();
            if (leftArm != null) { leftRest = leftArm.localPosition; leftRotation = leftArm.localRotation; leftRenderer = leftArm.GetComponent<Renderer>(); }
            if (rightArm != null) { rightRest = rightArm.localPosition; rightRotation = rightArm.localRotation; rightRenderer = rightArm.GetComponent<Renderer>(); }
            UpdateArm(leftArm, leftRenderer, leftRest, leftRotation, 0f);
            UpdateArm(rightArm, rightRenderer, rightRest, rightRotation, 0f);
        }

        private void LateUpdate()
        {
            if (controller == null || leftArm == null || rightArm == null) return;
            Vector3 v = controller.velocity;
            float speed = new Vector2(v.x, v.z).magnitude;
            bool moving = speed > 0.2f;
            if (moving) phase += Time.deltaTime * bobFrequency * Mathf.Clamp(speed / 3.5f, 0.35f, 1.4f);
            float stride = Mathf.Sin(phase);
            float leftTarget = moving ? Mathf.SmoothStep(0f, 1f, Mathf.Max(0f, stride)) : 0f;
            float rightTarget = moving ? Mathf.SmoothStep(0f, 1f, Mathf.Max(0f, -stride)) : 0f;
            leftLift = Mathf.MoveTowards(leftLift, leftTarget, transitionSpeed * Time.deltaTime);
            rightLift = Mathf.MoveTowards(rightLift, rightTarget, transitionSpeed * Time.deltaTime);
            UpdateArm(leftArm, leftRenderer, leftRest, leftRotation, leftLift);
            UpdateArm(rightArm, rightRenderer, rightRest, rightRotation, rightLift);
            if (!moving && leftLift == 0f && rightLift == 0f) phase = 0f;
        }

        private void UpdateArm(Transform arm, Renderer armRenderer, Vector3 rest, Quaternion rotation, float lift)
        {
            if (arm == null) return;
            arm.localPosition = rest + Vector3.up * Mathf.Lerp(-hiddenDrop, bobHeight, lift);
            arm.localRotation = Quaternion.Euler(-swingDegrees * lift, 0f, 0f) * rotation;
            if (armRenderer != null) armRenderer.enabled = lift > 0.001f;
        }
    }
}
