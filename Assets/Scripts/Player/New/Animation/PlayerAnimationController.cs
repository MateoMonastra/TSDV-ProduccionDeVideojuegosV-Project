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
        private static readonly int IsOnGround = Animator.StringToHash("IsOnGround");
        
        [SerializeField] Animator animator;
        
        public void SetJumpAnimation(bool isJumping)
        {
            animator.SetBool(IsJumping,isJumping);
        }
        
        public void SetFallAnimation( bool isFalling)
        {
            animator.SetBool(IsFalling,isFalling);
        }
        
        public void SetDeadAnimation()
        {
            animator.SetTrigger(IsDead);
        }
        
        public void SetFinishFallAnimation()
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
