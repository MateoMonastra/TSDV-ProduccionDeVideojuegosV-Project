using UnityEngine;

namespace CheckPoint
{
    public class CheckPointSignalAnimationController : MonoBehaviour
    {
        private static readonly int IsActive = Animator.StringToHash("isActive");

        [SerializeField] Animator animator;
        [SerializeField] GameObject vfxPrefab;
        [SerializeField] AK.Wwise.Event sfx;

        public void SetActivateAnimation(bool isActive)
        {
            animator.SetBool(IsActive, isActive);
        }

        public void OnFinishAnimation()
        {
            vfxPrefab?.SetActive(true);
            sfx?.Post(gameObject);
        }
    }
}