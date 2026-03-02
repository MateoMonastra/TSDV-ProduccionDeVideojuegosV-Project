using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.Beetle
{
    public class BeetleAnimationController : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Animator animator;

        private readonly string _deadTrigger = "Dead";
        private readonly string _attackTrigger = "Attack";

        void Reset()
        {
            animator = GetComponent<Animator>();
        }

        void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        public void PlayAttack()
        {
            if (animator == null) return;
            
            animator.SetTrigger(_attackTrigger);
        }
        
        public void PlayDead()
        {
            if (animator == null) return;

            animator.SetTrigger(_deadTrigger);
        }
    }
}