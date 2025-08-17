using FSM;
using UnityEngine;

namespace Player.New
{
    public class JumpGround : LocomotionState
    {
        public const string ToFall = "JumpGround->Fall";
        public const string ToJumpAir = "JumpGround->JumpAir";

        private readonly float _airDetectDelay;
        private float _t;

        public JumpGround(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req, float airDetectDelay = 0.04f)
            : base(m, mdl, cam, req) { _airDetectDelay = airDetectDelay; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;

            Model.JumpWasPureVertical = Model.RawMoveInput.sqrMagnitude <= 1e-4f;

            if (Model.JumpsLeft > 0) Model.JumpsLeft--;
            var v = Motor.Velocity;
            if (v.y < 0f) v.y = 0f;
            v.y += Model.JumpSpeed;
            Motor.SetVelocity(v);
            // TODO anim/sfx salto
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);

            if (_t >= _airDetectDelay && !Motor.IsGrounded)
                RequestTransition?.Invoke(ToFall);
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump" &&
                values[1] is bool pressed && pressed)
            {
                if (!Motor.IsGrounded && Model.JumpsLeft > 0)
                    RequestTransition?.Invoke(ToJumpAir);
            }
        }
    }
}