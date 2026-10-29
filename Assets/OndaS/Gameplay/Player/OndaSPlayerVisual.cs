using UnityEngine;
using UnityEngine.Rendering;
using OndaS.Visual;

namespace OndaS.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class OndaSPlayerVisual : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private bool firstPersonShadowsOnly = true;
        private CharacterController controller;
        private OndaSLookDevWalkthrough movement;
        private string currentState = "Idle";

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            movement = GetComponent<OndaSLookDevWalkthrough>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null) return;
            animator.applyRootMotion = false;
            if (firstPersonShadowsOnly)
                foreach (var renderer in animator.GetComponentsInChildren<Renderer>())
                    renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }

        private void LateUpdate()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            Vector3 velocity = controller.velocity;
            bool nowMoving = new Vector2(velocity.x, velocity.z).sqrMagnitude > 0.04f;
            string nextState = !nowMoving ? "Idle" : movement != null && movement.IsSprinting ? "Run" : "Walk";
            if (nextState == currentState) return;
            currentState = nextState;
            animator.CrossFade(nextState, 0.12f);
        }
    }
}
