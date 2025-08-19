using FSM;
using UnityEngine;

namespace Player.New
{

    public class SpinCharge : FinishableState
    {
        public const string ToRelease = "SpinCharge->SpinRelease";
        public const string ToIdle    = "SpinCharge->AttackIdle";

        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly Transform _cam;
        private readonly MyKinematicMotor _motor;
        private readonly PlayerAnimationController _anim;

        private float _t;                 
        private bool  _released;           
        private bool  _canStart;            

        public SpinCharge(PlayerModel model,
                          System.Action<string> request,
                          Transform cam,
                          MyKinematicMotor motor,
                          PlayerAnimationController anim = null)
        {
            _model = model;
            _req   = request;
            _cam   = cam;
            _motor = motor;
            _anim  = anim;
        }

        public override void Enter()
        {
            base.Enter();
            
            _canStart = _motor.IsGrounded && !_model.SpinOnCooldown;
            if (!_canStart)
            {
                _req?.Invoke(ToIdle);
                Finish();
                return;
            }

            _t = 0f;
            _released = false;
            
            _model.actionMoveSpeedMultiplier = _model.SpinMoveSpeedMultiplierWhileCharging;
            _model.aimLockActive = false;

            _anim?.SetCombatActive(true);
            _anim?.SetSpinCharging(true);
        }

        public override void Exit()
        {
            base.Exit();
            _anim?.SetSpinCharging(false);
            _anim?.SetCombatActive(false);
            _model.actionMoveSpeedMultiplier = 1f;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            if (!_canStart) return;

            _t += dt;
            
            if (_released && _t >= _model.SpinChargeMinTime)
            {
                _req?.Invoke(ToRelease);
                Finish();
                return;
            }
            
            if (_released && _t < _model.SpinChargeMinTime)
            {
                _req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >=1 } && values[0] is string cmd && cmd == "AttackHeavyReleased")
                _released = true;
        }
    }
}
