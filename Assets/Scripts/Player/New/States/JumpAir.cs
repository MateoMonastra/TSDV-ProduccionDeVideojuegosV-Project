using FSM;
using UnityEngine;

namespace Player.New
{
    public class JumpAir : LocomotionState
    {
        public const string ToFall = "JumpAir->Fall";
        private readonly float _airDetectDelay;
        private float _t;
        private readonly PlayerAnimationController _anim;
        
        public JumpAir(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
            float airDetectDelay = 0.02f, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req) { _airDetectDelay = airDetectDelay; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            
            _anim?.TriggerDoubleJump();
            _anim?.SetGrounded(false);
            _anim?.SetFalling(false);
            
            _t = 0f;

            if (Model.JumpsLeft > 0) Model.JumpsLeft--;
            var v = Motor.Velocity;
            if (v.y < 0f) v.y = 0f;
            v.y += Model.JumpSpeed;
            Motor.SetVelocity(v);
            // TODO anim/sfx segundo salto
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);

            if (_t >= _airDetectDelay && !Motor.IsGrounded)
                RequestTransition?.Invoke(ToFall);
        }
    }
}