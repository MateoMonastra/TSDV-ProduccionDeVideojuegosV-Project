using FSM;
using UnityEngine;

namespace Player.New
{
    public class WalkIdle : LocomotionState
    {
        public const string ToJump     = "WalkIdle->JumpGround";
        public const string ToFall     = "WalkIdle->Fall";
        public const string ToSprint   = "WalkIdle->Sprint";

        private readonly System.Action<bool> _onWalk;
        private readonly float _coyoteTime;
        private float _timeSinceUngrounded;
        private readonly PlayerAnimationController _anim;

        public WalkIdle(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
                        System.Action<bool> onWalk = null, float coyoteTime = 0.12f, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        {
            _onWalk = onWalk;
            _coyoteTime = Mathf.Max(0f, coyoteTime);
            _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();
            _timeSinceUngrounded = 0f;
            _anim?.SetGrounded(true);
            _anim?.SetFalling(false);
            _anim?.SetWalking(false);
        }

        public override void Exit()
        {
            base.Exit();
            _onWalk?.Invoke(false);
            _anim?.SetWalking(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            // ===== INPUT a mundo (como antes) =====
            Vector3 up = Motor.CharacterUp;
            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.moveInputWorld = camFwd * Model.rawMoveInput.y + camRight * Model.rawMoveInput.x;
            if (Model.moveInputWorld.sqrMagnitude > 1e-6f) Model.moveInputWorld.Normalize();

            bool grounded = Motor.IsGrounded;
            if (!grounded)
            {
                _timeSinceUngrounded += dt;
                if (_timeSinceUngrounded > _coyoteTime)
                {
                    RequestTransition?.Invoke(ToFall);
                    return;
                }
            }
            else
            {
                _timeSinceUngrounded = 0f;
            }

            // ===== Locomoción (idle/walk) =====
            ApplyLocomotion(dt, inAir: false);
            var hv = Motor.Velocity; hv.y = 0f;
            bool walking = hv.sqrMagnitude > 0.0001f && Model.moveInputWorld.sqrMagnitude > 0f;

            _onWalk?.Invoke(walking);
            _anim?.SetWalking(walking);

            // ===== Sprint window management (↓ movido desde PlayerAgent.Update) =====
            if (Model.sprintArmed)
            {
                Model.sprintArmTimeLeft -= dt;
                if (Model.sprintArmTimeLeft <= 0f)
                {
                    // se cerró la ventana
                    Model.sprintArmed = false;
                    Model.sprintHoldCounter = 0f;
                }
                else
                {
                    // acumular hold solo mientras se mantiene el botón
                    if (Model.dashHeld) Model.sprintHoldCounter += dt;
                    else                Model.sprintHoldCounter  = 0f;

                    // condiciones para iniciar sprint:
                    // - alcanzó el tiempo de hold
                    // - hay input de movimiento (walking)
                    // - estamos en suelo
                    if (Model.sprintHoldCounter >= Model.sprintHoldTime &&
                        walking &&
                        grounded)
                    {
                        // cerrar ventana y limpiar contador ANTES de irnos
                        Model.sprintArmed = false;
                        Model.sprintHoldCounter = 0f;

                        RequestTransition?.Invoke(ToSprint);
                        return;
                    }
                }
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump")
            {
                bool pressed = (bool)values[1];
                if (pressed && (Motor.IsGrounded || _timeSinceUngrounded <= _coyoteTime) && Model.jumpsLeft > 0)
                {
                    RequestTransition?.Invoke(ToJump);
                    return;
                }
            }
        }
    }
}
