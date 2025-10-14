using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimationController : MonoBehaviour
    {
        private static readonly int IsChaseHash   = Animator.StringToHash("IsChase");
        private static readonly int IsDamagedHash = Animator.StringToHash("IsDamaged");
        private static readonly int AttackHash    = Animator.StringToHash("Attack");
        private static readonly int AttackHitHash = Animator.StringToHash("AttackHit");
        private static readonly int DeathHash     = Animator.StringToHash("Death");

        [SerializeField] private Animator animator;

        private void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
        }

        public void SetWalkAnimation(bool isWalk)
        {
            animator?.SetBool(IsChaseHash, isWalk);
        }

        public void SetDamagedAnimation(bool isDamaged)
        {
            animator?.SetBool(IsDamagedHash, isDamaged);
        }

        public void SetAttackAnimation()
        {
            animator?.SetTrigger(AttackHash);
        }

        public void SetAttackHitAnimation()
        {
            animator?.SetTrigger(AttackHitHash);
        }

        public void SetDeathAnimation()
        {
            animator?.SetTrigger(DeathHash);
        }
    }
}