using UnityEngine;

namespace OndaS.Gameplay
{
    // Uses the existing Animator and animation states supplied by the environment kit.
    public sealed class OndaSDoor : MonoBehaviour
    {
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private string openingState = "Opening";
        [SerializeField] private string closingState = "Closing";
        [SerializeField] private bool initiallyOpen;

        private bool isOpen;
        private float availableAt;

        // Several colliders can control different leaves of one animated door.
        // The component on the Animator owns the shared state for those leaves.
        private OndaSDoor StateOwner
        {
            get
            {
                var owner = doorAnimator != null ? doorAnimator.GetComponent<OndaSDoor>() : null;
                return owner != null && owner.doorAnimator == doorAnimator ? owner : this;
            }
        }

        public bool IsOpen => StateOwner.isOpen;
        public bool CanInteract
        {
            get
            {
                var owner = StateOwner;
                return owner.doorAnimator != null && owner.doorAnimator.isActiveAndEnabled && Time.time >= owner.availableAt;
            }
        }

        private void Awake() { isOpen = initiallyOpen; }

        public bool Toggle()
        {
            var owner = StateOwner;
            if (owner != this) return owner.Toggle();
            if (!CanInteract) return false;
            string state = isOpen ? closingState : openingState;
            int stateHash = Animator.StringToHash(state);
            if (!doorAnimator.HasState(0, stateHash))
            {
                Debug.LogWarning("No se encontró la animación '" + state + "' en " + name, this);
                return false;
            }
            float duration = 0.5f;
            var controller = doorAnimator.runtimeAnimatorController;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                    if (clip != null && clip.name == state) { duration = clip.length; break; }
            }
            availableAt = Time.time + duration / Mathf.Max(0.01f, Mathf.Abs(doorAnimator.speed));
            doorAnimator.Play(stateHash, 0, 0f);
            isOpen = !isOpen;
            return true;
        }
    }
}
