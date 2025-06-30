using UnityEngine;

namespace Player.New
{
    public class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int IsFalling = Animator.StringToHash("IsFalling");
        private static readonly int MovementValueX = Animator.StringToHash("MovementValueX");
        private static readonly int MovementValueZ = Animator.StringToHash("MovementValueZ");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        
        [SerializeField] Animator animator;
        
        public void SetJumpAnimation()
        {
            animator.SetTrigger(IsJumping);
        }
        
        public void SetFallAnimation()
        {
            animator.SetTrigger(IsFalling);
        }
        
        public void SetDeadAnimation()
        {
            animator.SetTrigger(IsDead);
        }

        public void SetMovementVelocity( float movementValueX, float movementValueZ)
        {
            animator.SetFloat(MovementValueX,Mathf.Abs(movementValueX));
            animator.SetFloat(MovementValueZ,Mathf.Abs(movementValueZ));
        }
    }
}
