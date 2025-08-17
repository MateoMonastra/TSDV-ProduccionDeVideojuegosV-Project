using FSM;
using UnityEngine;

namespace Player.New
{
    public class SpinRelease : FinishableState
    {
        public const string ToIdle = "SpinRelease->AttackIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;

        private float _t;

        public System.Action<float> OnSpinCooldownUI;

        private readonly PlayerAnimationController _anim;
        public SpinRelease(MyKinematicMotor m, PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null)
        { _m = m; _model = model; _req = request; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;

            _model.LocomotionBlocked = true;
            _model.AimLockActive = true;
            _model.ActionMoveSpeedMultiplier = 0f;

            _model.SpinOnCooldown = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            OnSpinCooldownUI?.Invoke(_model.SpinCooldownLeft);
            
            _anim?.SetSpinCharging(false);
            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;
        }

        public override void Exit()
        {
            base.Exit();
            _model.LocomotionBlocked = false;
            _model.AimLockActive = false;
            _model.ActionMoveSpeedMultiplier = 1f;
            _model.AimLockDirection = Vector3.zero;
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            if (Mathf.Abs(_t - _model.SpinDuration * 0.5f) < 0.02f)
            {
                // TODO daño en área (_model.SpinRadius) + push (_model.SpinPushDistance)
            }

            if (_model.SpinOnCooldown)
            {
                _model.SpinCooldownLeft = Mathf.Max(0f, _model.SpinCooldownLeft - dt);
                OnSpinCooldownUI?.Invoke(_model.SpinCooldownLeft);
                if (_model.SpinCooldownLeft <= 0f) _model.SpinOnCooldown = false;
            }

            if (_t >= _model.SpinDuration + _model.SpinPostStun)
            {
                _req?.Invoke(ToIdle);
                Finish();
            }
        }
        
        private void OnSpinDamageEvent()
        {
            // aplicar daño en área del 360° en el frame del evento
        }
    }
}
