using UnityEngine;

namespace UI
{
    /// <summary>Controla un icono de pickup con 2 animaciones: Get (aparece) y Use (desaparece).</summary>
    public class PickupWidget : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string getStateName = "Get";
        [SerializeField] private string useStateName = "Use";

        public void PlayGet()
        {
            gameObject.SetActive(true);
            if (animator) animator.CrossFadeInFixedTime(getStateName, 0f, 0, 0f);
        }

        public void PlayUse()
        {
            if (animator) animator.CrossFadeInFixedTime(useStateName, 0f, 0, 0f);
        }
        
        public void OnUseAnimationEnd()
        {
            gameObject.SetActive(false);
        }
    }
}