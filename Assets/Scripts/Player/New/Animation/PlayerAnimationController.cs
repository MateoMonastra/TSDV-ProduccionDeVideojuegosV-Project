using System;
using UnityEngine;

namespace Player.New
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Animator _anim;
        
        static readonly int pIsWalking   = Animator.StringToHash("IsWalking");
        static readonly int pIsGrounded  = Animator.StringToHash("IsGrounded");
        static readonly int pIsFalling   = Animator.StringToHash("IsFalling");
        static readonly int tJump        = Animator.StringToHash("Jump");
        static readonly int tDoubleJump  = Animator.StringToHash("DoubleJump");
        static readonly int tLand        = Animator.StringToHash("Land");
        static readonly int tDash        = Animator.StringToHash("Dash");
        static readonly int tAttack1     = Animator.StringToHash("Attack1");
        static readonly int tAttack2     = Animator.StringToHash("Attack2");
        static readonly int tAttack3     = Animator.StringToHash("Attack3");
        static readonly int tVerticalStart  = Animator.StringToHash("VerticalStart");
        static readonly int tVerticalImpact = Animator.StringToHash("VerticalImpact");
        static readonly int bSpinCharging   = Animator.StringToHash("SpinCharging");
        static readonly int tSpinRelease    = Animator.StringToHash("SpinRelease");

        void Reset()
        {
            _anim = GetComponent<Animator>();
        }
        
        public void SetWalking(bool v)   { if (_anim) _anim.SetBool(pIsWalking, v); }
        public void SetGrounded(bool v)  { if (_anim) _anim.SetBool(pIsGrounded, v); }
        public void SetFalling(bool v)   { if (_anim) _anim.SetBool(pIsFalling, v); }

        public void TriggerJump()        { if (_anim) _anim.SetTrigger(tJump); }
        public void TriggerDoubleJump()  { if (_anim) _anim.SetTrigger(tDoubleJump); }
        public void TriggerLand()        { if (_anim) _anim.SetTrigger(tLand); }
        public void TriggerDash()        { if (_anim) _anim.SetTrigger(tDash); }

        public void TriggerAttack1()     { if (_anim) _anim.SetTrigger(tAttack1); }
        public void TriggerAttack2()     { if (_anim) _anim.SetTrigger(tAttack2); }
        public void TriggerAttack3()     { if (_anim) _anim.SetTrigger(tAttack3); }

        public void TriggerVerticalStart(){ if (_anim) _anim.SetTrigger(tVerticalStart); }
        public void TriggerVerticalImpact(){ if (_anim) _anim.SetTrigger(tVerticalImpact); }

        public void SetSpinCharging(bool v) { if (_anim) _anim.SetBool(bSpinCharging, v); }
        public void TriggerSpinRelease()    { if (_anim) _anim.SetTrigger(tSpinRelease); }
        
        
        //Eventos de animacion para el futuro cuando metamos audio
        public Action OnAnim_AttackHit;       
        public Action OnAnim_VerticalImpact;  
        public Action OnAnim_SpinDamage;      
        public Action OnAnim_Footstep;        
        
        public void AnimEvent_AttackHit()      { OnAnim_AttackHit?.Invoke(); }
        public void AnimEvent_VerticalImpact() { OnAnim_VerticalImpact?.Invoke(); }
        public void AnimEvent_SpinDamage()     { OnAnim_SpinDamage?.Invoke(); }
        public void AnimEvent_Footstep()       { OnAnim_Footstep?.Invoke(); }
    }
}
