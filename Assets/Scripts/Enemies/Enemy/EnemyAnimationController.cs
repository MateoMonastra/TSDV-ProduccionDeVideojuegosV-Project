using UnityEngine;

namespace Enemy
{
    public class EnemyAnimationController : MonoBehaviour
    {
        private static readonly int IsChase = Animator.StringToHash("IsChase");
        private static readonly int Attack1 = Animator.StringToHash("Attack");
        [SerializeField] Animator animator;

        public void SetWalkAnimation(bool isWalk)
        {
            animator.SetBool(IsChase, isWalk);
        }

        public void SetAttackAnimation()
        {
            animator.SetTrigger(Attack1);
        }
    }
}
