using System;
using FSM;
using UnityEngine;

namespace Player.New
{
    public class WalkIdle : LocomotionState
    {
        public const string ToJump = "WalkIdle->JumpGround";
        public const string ToFall = "WalkIdle->Fall";

        private readonly Action<bool> _onWalk;
        private readonly float _coyoteTime;
        private float _ungroundedTimer;
        private bool _jumpRequested;

        public WalkIdle(MyKinematicMotor m, PlayerModel mdl, Transform cam, Action<string> req,
            Action<bool> onWalk = null, float coyoteTime = 0.12f)
            : base(m, mdl, cam, req)
        { _onWalk = onWalk; _coyoteTime = coyoteTime; }

        public override void Enter()
        {
            base.Enter();
            Model.ResetJumps();
            _ungroundedTimer = 0f;
            _jumpRequested = false;
            _onWalk?.Invoke(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            ApplyLocomotion(dt, inAir: false);

            var hv = Motor.Velocity; hv.y = 0f;
            _onWalk?.Invoke(hv.sqrMagnitude > 0.01f);

            if (_jumpRequested && Motor.IsGrounded && Model.JumpsLeft > 0)
            {
                _jumpRequested = false;
                RequestTransition?.Invoke(ToJump);
                return;
            }

            if (Motor.IsGrounded) _ungroundedTimer = 0f;
            else
            {
                _ungroundedTimer += dt;
                if (_ungroundedTimer >= _coyoteTime)
                    RequestTransition?.Invoke(ToFall);
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump" &&
                values[1] is bool pressed && pressed) _jumpRequested = true;
        }
    }
}