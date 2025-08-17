using FSM;
using UnityEngine;

namespace Player.New
{
    public class Fall : LocomotionState
    {
        public const string ToWalkIdle = "Fall->WalkIdle";
        private readonly float _settleTime;
        private float _groundedTimer;
        
        private readonly PlayerAnimationController _anim;
        public Fall(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
            float settleTime = 0.04f, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req) { _settleTime = settleTime; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _anim?.SetFalling(true);
        }
        public override void Tick(float dt)
        {
            base.Tick(dt);

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);

            if (Motor.IsGrounded)
            {
                _groundedTimer += dt;
                if (_groundedTimer >= _settleTime)
                {
                    _anim?.SetFalling(false);
                    _anim?.TriggerLand();
                    _anim?.SetGrounded(true);
                    _anim?.SetWalking(false);
                    
                    var v = Motor.Velocity; v.x = 0f; v.z = 0f;
                    Motor.SetVelocity(v);
                    RequestTransition?.Invoke(ToWalkIdle);
                }
            }
            else _groundedTimer = 0f;
        }
    }
}