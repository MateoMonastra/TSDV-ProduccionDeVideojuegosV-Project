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
            _anim?.SetGrounded(true);
            _anim?.SetFalling(false);
            _anim?.SetWalking(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            ApplyLocomotion(dt, inAir: false);

            var hv = Motor.Velocity; hv.y = 0f;
            bool walking = hv.sqrMagnitude > 0.01f;
            _onWalk?.Invoke(walking);
            _anim?.SetWalking(walking);
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump" &&
                values[1] is bool pressed && pressed) _jumpRequested = true;
        }
    }
}