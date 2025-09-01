using System;
using FSM;
using UnityEngine;

namespace Player.New
{
    public class WalkIdle : LocomotionState
    {
        public const string ToJump = "WalkIdle->JumpGround";
        public const string ToFall = "WalkIdle->Fall";
        public const string ToSprint = "WalkIdle->Sprint";

        private readonly Action<bool> _onWalk;
        private readonly PlayerAnimationController _anim;
        private readonly float _coyoteTime;
        private float _ungroundedTimer;
        private bool _jumpRequested;

        public WalkIdle(MyKinematicMotor m, PlayerModel mdl, Transform cam, Action<string> req,
            Action<bool> onWalk = null, float coyoteTime = 0.12f, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        { _onWalk = onWalk; _coyoteTime = coyoteTime; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            Model.ResetJumps();
            _ungroundedTimer = 0f;
            _jumpRequested = false;
            _anim?.SetGrounded(true);
            _anim?.SetFalling(false);
            _anim?.SetWalking(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            // 1) Actualizar MoveInputWorld (cámara-relativo) para dash/anim
            Vector3 up = Motor.CharacterUp;
            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.moveInputWorld = camFwd * Model.rawMoveInput.y + camRight * Model.rawMoveInput.x;
            if (Model.moveInputWorld.sqrMagnitude > 1e-6f) Model.moveInputWorld.Normalize();

            // 2) Locomoción en suelo (incluye stop instantáneo)
            ApplyLocomotion(dt, inAir: false);

            // 3) Anim walk/idle
            var hv = Motor.Velocity; hv.y = 0f;
            bool walking = hv.sqrMagnitude > 0.01f;
            _onWalk?.Invoke(walking);
            _anim?.SetWalking(walking);

            // 4) Jump request
            if (_jumpRequested && Motor.IsGrounded && Model.jumpsLeft > 0)
            {
                _jumpRequested = false;
                RequestTransition?.Invoke(ToJump);
                return;
            }

            // 5) Coyote → Fall
            if (Motor.IsGrounded) _ungroundedTimer = 0f;
            else
            {
                _ungroundedTimer += dt;
                if (_ungroundedTimer >= _coyoteTime)
                    RequestTransition?.Invoke(ToFall);
            }
            
            if (Model.sprintArmed && Model.dashHeld && walking && Motor.IsGrounded)
            {
                RequestTransition?.Invoke(ToSprint);
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump" &&
                values[1] is bool pressed && pressed)
                _jumpRequested = true;
        }
    }
}
