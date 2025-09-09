using Platforms;
using UnityEngine;
using UnityEngine.Events;

namespace Hazards.BlowerBridge
{
    public class Button : MonoBehaviour, IBreakable
    {
        public UnityEvent onActivate;
        public UnityEvent onDeactivate;

        private static readonly int IdleTrigger = Animator.StringToHash("Idle");
        private static readonly int PressTrigger = Animator.StringToHash("Pressed");

        [SerializeField] private bool resetable = true;
        [SerializeField] private float cooldown = 2f;
        [SerializeField] private Animator animator;

        private bool _isOnCooldown = false;

        private void ActivateButton()
        {
            _isOnCooldown = true;

            if (animator)
            {
                animator.ResetTrigger(IdleTrigger);
                animator.SetTrigger(PressTrigger);
            }

            onActivate?.Invoke();

            if (resetable)
                Invoke(nameof(ResetButton), cooldown);
        }

        private void ResetButton()
        {
            _isOnCooldown = false;

            if (!animator) return;
            animator.ResetTrigger(PressTrigger);
            animator.SetTrigger(IdleTrigger);

            onDeactivate?.Invoke();
        }

        public void Break()
        {
            if (_isOnCooldown) return;
            ActivateButton();
        }
    }
}