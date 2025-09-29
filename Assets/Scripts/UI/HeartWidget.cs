using UnityEngine;

namespace UI
{
    public enum HeartState { Empty = 0, Full = 1 }
    public enum TransitionCause { Damage, Heal }

    public class HeartWidget : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private static readonly int HashDamage    = Animator.StringToHash("Damage");
        private static readonly int HashHeal      = Animator.StringToHash("Heal");

        private HeartState _last;

        public void Init(HeartState initial)
        {
            _last = initial;
        }

        public void Play(HeartState next, TransitionCause cause)
        {
            if (!animator) { _last = next; return; }
            
            if (cause == TransitionCause.Damage) animator.SetTrigger(HashDamage);
            else if (cause == TransitionCause.Heal) animator.SetTrigger(HashHeal);

            _last = next;
        }

        public HeartState LastState => _last;
    }
}