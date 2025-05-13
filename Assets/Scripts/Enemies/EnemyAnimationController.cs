using UnityEngine;

namespace Enemies
{
    public class EnemyAnimationController : MonoBehaviour
    {
        private static readonly int IsChase = Animator.StringToHash("IsChase");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int AttackHit = Animator.StringToHash("AttackHit");
        
        [SerializeField] Animator animator;

        public void SetWalkAnimation(bool isWalk)
        {
            animator.SetBool(IsChase, isWalk);
        }

        public void SetAttackAnimation()
        {
            animator.SetTrigger(Attack);
        }
        
        public void SetAttackHitAnimation()
        {
            animator.SetTrigger(AttackHit);
        }
    }
}
