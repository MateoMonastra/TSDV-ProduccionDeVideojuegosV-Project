using System;
using UnityEngine;

namespace Player.New
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Refs")] [SerializeField] private Animator _anim;

        [Header("Animator Layers")] [SerializeField]
        private string _combatLayerName = "Combat";

        private int _combatLayer = -1;

        // Params
        static readonly int pIsWalking = Animator.StringToHash("IsWalking");
        static readonly int pIsGrounded = Animator.StringToHash("IsGrounded");
        static readonly int pIsFalling = Animator.StringToHash("IsFalling");
        static readonly int pIsInteracting = Animator.StringToHash("IsInteracting");
        static readonly int tJump = Animator.StringToHash("Jump");
        static readonly int tDoubleJump = Animator.StringToHash("DoubleJump");
        static readonly int tLand = Animator.StringToHash("Land");
        static readonly int tDash = Animator.StringToHash("Dash");
        static readonly int tAttack1 = Animator.StringToHash("Attack1");
        static readonly int tAttack2 = Animator.StringToHash("Attack2");
        static readonly int tAttack3 = Animator.StringToHash("Attack3");
        static readonly int tVerticalStart = Animator.StringToHash("VerticalStart");
        static readonly int tVerticalImpact = Animator.StringToHash("VerticalImpact");
        static readonly int bSpinCharging = Animator.StringToHash("SpinCharging");
        static readonly int tSpinRelease = Animator.StringToHash("SpinRelease");
        static readonly int tKnockdown = Animator.StringToHash("Knockdown");
        static readonly int tGetUp = Animator.StringToHash("GetUp");
        static readonly int IsDie = Animator.StringToHash("Die");
        static readonly int IsHit = Animator.StringToHash("Hit");


        [SerializeField] private bool _debugAnimEvents = false;

        void Awake()
        {
            if (!_anim) _anim = GetComponent<Animator>();
            _combatLayer = _anim ? _anim.GetLayerIndex(_combatLayerName) : -1;
            if (_combatLayer < 0)
                Debug.LogWarning($"[Animator] No existe la layer '{_combatLayerName}'.");
        }

        void OnEnable()
        {
            SetCombatActive(false);
        }

        // ------- API Animator -------
        public void SetWalking(bool v)
        {
            if (_anim) _anim.SetBool(pIsWalking, v);
        }

        public void SetInteracting(bool v)
        {
            if(_anim) _anim.SetBool(pIsInteracting, v);
        }
        
        public void SetGrounded(bool v)
        {
            if (_anim) _anim.SetBool(pIsGrounded, v);
        }

        public void SetFalling(bool v)
        {
            if (_anim) _anim.SetBool(pIsFalling, v);
        }

        public void TriggerJump()
        {
            if (_anim) _anim.SetTrigger(tJump);
        }

        public void TriggerDoubleJump()
        {
            if (_anim) _anim.SetTrigger(tDoubleJump);
        }

        public void TriggerLand()
        {
            if (_anim) _anim.SetTrigger(tLand);
        }

        public void TriggerDash()
        {
            if (_anim) _anim.SetTrigger(tDash);
        }

        public void TriggerAttack1()
        {
            if (_anim) _anim.SetTrigger(tAttack1);
        }

        public void TriggerAttack2()
        {
            if (_anim) _anim.SetTrigger(tAttack2);
        }

        public void TriggerAttack3()
        {
            if (_anim) _anim.SetTrigger(tAttack3);
        }

        public void TriggerVerticalStart()
        {
            if (_anim) _anim.SetTrigger(tVerticalStart);
        }

        public void TriggerVerticalImpact()
        {
            if (_anim) _anim.SetTrigger(tVerticalImpact);
        }

        public void SetSpinCharging(bool v)
        {
            if (_anim) _anim.SetBool(bSpinCharging, v);
        }

        public void TriggerSpinRelease()
        {
            if (_anim) _anim.SetTrigger(tSpinRelease);
        }

        public void TriggerKnockdown()
        {
            if (_anim) _anim.SetTrigger(tKnockdown);
        }

        public void TriggerGetUp()
        {
            if (_anim) _anim.SetTrigger(tGetUp);
        }

        public void TriggerDeath()
        {
            SetFalling(false);
            SetInteracting(false);
            SetWalking(false);
            _anim?.SetTrigger(IsDie);
        }

        public void TriggerHit() => _anim?.SetTrigger(IsHit);


        // ------- Layer helpers -------
        public void SetCombatActive(bool active)
        {
            if (_anim && _combatLayer >= 0)
                _anim.SetLayerWeight(_combatLayer, active ? 1f : 0f);
        }

        [ContextMenu("Log Animator Layers")]
        void LogAnimatorLayers()
        {
            if (!_anim) return;
            for (int i = 0; i < _anim.layerCount; i++)
                Debug.Log($"Layer[{i}] '{_anim.GetLayerName(i)}' weight={_anim.GetLayerWeight(i):0.00}");
        }

        // ------- Animation Events -------
        public Action OnAnim_AttackHit;
        public Action OnAnim_VerticalImpact;
        public Action OnAnim_SpinDamage;
        public Action OnAnim_Footstep;
        public Action OnAnim_DeathFinished;

        public void AnimEvent_AttackHit()
        {
            if (_debugAnimEvents) Debug.Log("[AnimEvent] AttackHit");
            OnAnim_AttackHit?.Invoke();
        }

        public void AnimEvent_VerticalImpact()
        {
            if (_debugAnimEvents) Debug.Log("[AnimEvent] VerticalImpact");
            OnAnim_VerticalImpact?.Invoke();
        }

        public void AnimEvent_SpinDamage()
        {
            if (_debugAnimEvents) Debug.Log("[AnimEvent] SpinDamage");
            OnAnim_SpinDamage?.Invoke();
        }

        public void AnimEvent_Footstep()
        {
            if (_debugAnimEvents) Debug.Log("[AnimEvent] Footstep");
            OnAnim_Footstep?.Invoke();
        }

        public void OnAnimEvent_DeathFinished()
        {
            OnAnim_DeathFinished?.Invoke();
        }
    }
}