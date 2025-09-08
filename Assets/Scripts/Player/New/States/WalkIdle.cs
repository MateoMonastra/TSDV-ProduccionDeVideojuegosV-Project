using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Locomoción en suelo: movimiento orientado por cámara, coyote para transición a Fall
    /// y manejo del salto (suelo o dentro del coyote).
    /// </summary>
    public class WalkIdle : LocomotionState
    {
        public const string ToJump = "ToJump";
        public const string ToFall = "ToFall";
        public const string ToSprint = "ToSprint";

        private float _timeSinceUngrounded;
        private int _ungroundedFrames;

        private readonly PlayerAnimationController _anim;

        public WalkIdle(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
            PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        {
            _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();
            _timeSinceUngrounded = 0f;
            _ungroundedFrames = 0;
            
            Model.ResetJumps();

            _anim?.SetGrounded(true);
            _anim?.SetFalling(false);
            _anim?.SetWalking(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            
            UpdateMoveInputWorld();
            
            ApplyLocomotion(dt, inAir: false);

            _anim?.SetWalking(Model.RawMoveInput.sqrMagnitude > 1e-5f);
            
            if (!Motor.IsGrounded)
            {
                _timeSinceUngrounded += dt;
                _ungroundedFrames++;

                if (_timeSinceUngrounded > Model.CoyoteTime && _ungroundedFrames >= 2)
                {
                    RequestTransition?.Invoke(ToFall);
                    return;
                }
            }
            else
            {
                _timeSinceUngrounded = 0f;
                _ungroundedFrames = 0;
            }
            
            HandleSprintWindow(dt);
        }

        /// <summary>
        /// Si Dash armó la ventana: cuenta hold mientras se mantiene el botón de dash
        /// y hay intención de movimiento; cuando llega al tiempo de hold → entra en Sprint.
        /// La ventana caduca tras un tiempo. Si se pierde suelo, se cancela.
        /// </summary>
        private void HandleSprintWindow(float dt)
        {
            if (!Motor.IsGrounded)
            {
                Model.SprintArmed = false;
                Model.SprintHoldCounter = 0f;
                return;
            }

            if (!Model.SprintArmed)
                return;
            
            Model.SprintArmTimeLeft -= dt;
            if (Model.SprintArmTimeLeft <= 0f)
            {
                Model.SprintArmed = false;
                Model.SprintHoldCounter = 0f;
                return;
            }
            
            if (Model.DashHeld) Model.SprintHoldCounter += dt;
            else Model.SprintHoldCounter = 0f;
            
            bool hasMoveInput = Model.RawMoveInput.sqrMagnitude > 1e-5f;

            if (Model.SprintHoldCounter >= Model.SprintHoldTime && hasMoveInput)
            {
                RequestTransition?.Invoke(ToSprint);
                
                Model.SprintHoldCounter = 0f;
            }
        }


        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } &&
                values[0] is string cmd &&
                cmd == CommandKeys.Jump &&
                values[1] is bool pressed && pressed)
            {
                
                bool canJumpFromCoyote = _timeSinceUngrounded <= Model.CoyoteTime;
                
                if (Motor.IsGrounded || canJumpFromCoyote)
                {
                    if (Model.JumpsLeft > 0)
                    {
                        RequestTransition?.Invoke(ToJump);
                    }
                }
            }
        }
        
        
        private void UpdateMoveInputWorld()
        {
            Vector3 up = Motor.CharacterUp;

            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f)
                camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;

            Vector3 camRight = Vector3.Cross(up, camFwd);

            Model.MoveInputWorld = camFwd * Model.RawMoveInput.y + camRight * Model.RawMoveInput.x;
            if (Model.MoveInputWorld.sqrMagnitude > 1e-6f)
                Model.MoveInputWorld = Model.MoveInputWorld.normalized;
        }
    }
}