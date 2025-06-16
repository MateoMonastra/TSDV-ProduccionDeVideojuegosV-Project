using UnityEngine;
using UnityEngine.Events;

namespace Hazards.BlowerBridge
{
    public class Button : MonoBehaviour
    {
        public UnityEvent onActivate;

        private static readonly int IdleTrigger = Animator.StringToHash("Idle");
        private static readonly int PressTrigger = Animator.StringToHash("Pressed");

        [SerializeField] private float cooldown = 2f;
        [SerializeField] private Animator animator;

        private bool _isOnCooldown = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_isOnCooldown) return;

            Debug.Log("1");
            if (other.TryGetComponent(out HammerController hammer) && hammer.IsGroundSlamming)
            {
                Debug.Log("2");
                ActivateButton();
            }
            else
            {
                Debug.Log(other.gameObject.name);
            }
        }

        private void ActivateButton()
        {
            _isOnCooldown = true;
            Debug.Log("Activated button");

            if (animator)
            {
                animator.ResetTrigger(IdleTrigger);
                animator.SetTrigger(PressTrigger);
            }

            onActivate?.Invoke();
            Invoke(nameof(ResetButton), cooldown);
        }

        private void ResetButton()
        {
            _isOnCooldown = false;

            if (!animator) return;
            animator.ResetTrigger(PressTrigger);
            animator.SetTrigger(IdleTrigger);
        }
    }
}