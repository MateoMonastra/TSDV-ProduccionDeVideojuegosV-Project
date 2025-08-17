using FSM;
using UnityEngine;

namespace Player.New
{
    public class Fall : LocomotionState
    {
        public const string ToWalkIdle = "Fall->WalkIdle";
        public const string ToJumpAir  = "Fall->JumpAir";   // <<--- NUEVO

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

            // mantener MoveInputWorld actualizado
            Vector3 up = Motor.CharacterUp;
            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.moveInputWorld = camFwd * Model.rawMoveInput.y + camRight * Model.rawMoveInput.x;
            if (Model.moveInputWorld.sqrMagnitude > 1e-6f) Model.moveInputWorld.Normalize();

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.airHorizontalSpeed);

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

        // <<--- NUEVO: permite doble salto mientras estás en Fall
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump" &&
                values[1] is bool pressed && pressed)
            {
                if (!Motor.IsGrounded && Model.jumpsLeft > 0)
                {
                    RequestTransition?.Invoke(ToJumpAir);
                }
            }
        }
    }
}
