using UnityEngine;

namespace CheckPoint
{
    public class CheckPointSignalAnimationController : MonoBehaviour
    {
        private static readonly int IsActive = Animator.StringToHash("isActive");
        
        [SerializeField] Animator animator;

        public void SetActivateAnimation(bool isActive)
        {
            animator.SetBool(IsActive, isActive);
        }
    }
}
