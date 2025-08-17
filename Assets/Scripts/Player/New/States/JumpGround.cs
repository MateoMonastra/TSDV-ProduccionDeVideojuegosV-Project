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
        private readonly PlayerAnimationController _anim;

        public JumpGround(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
            float airDetectDelay = 0.04f, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req) { _airDetectDelay = airDetectDelay; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;

            Motor.ForceUnground(0.10f);

            Model.JumpWasPureVertical = Model.RawMoveInput.sqrMagnitude <= 1e-4f;
            if (Model.JumpsLeft > 0) Model.JumpsLeft--;

            var v = Motor.Velocity;
            if (v.y < 0f) v.y = 0f;
            v.y += Model.JumpSpeed;
            Motor.SetVelocity(v);

            _anim?.TriggerJump();
            _anim?.SetGrounded(false);
            _anim?.SetFalling(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Actualizar MoveInputWorld mientras estamos en el aire
            Vector3 up = Motor.CharacterUp;
            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.MoveInputWorld = camFwd * Model.RawMoveInput.y + camRight * Model.RawMoveInput.x;
            if (Model.MoveInputWorld.sqrMagnitude > 1e-6f) Model.MoveInputWorld.Normalize();

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
