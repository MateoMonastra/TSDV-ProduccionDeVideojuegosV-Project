using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Caída en aire: permite control horizontal, salto en aire (si queda stock) y
    /// vuelve a Idle tras un tiempo de “settle” con grounded estable (filtra parpadeos).
    /// </summary>
    public class Fall : LocomotionState
    {
        // IDs de transición limpias
        public const string ToWalkIdle = "ToWalkIdle";
        public const string ToJumpAir  = "ToJumpAir";
        
        private int _groundedFrames;

        private readonly PlayerAnimationController _anim;

        public Fall(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        {
            _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();
            _groundedFrames = 0;
            _anim?.SetFalling(true);
        }

        public override void Exit()
        {
            base.Exit();
            _anim?.SetFalling(false);
            _anim?.SetGrounded(true);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            // Mantener MoveInputWorld actualizado (usado por Dash u otros)
            UpdateMoveInputWorld();

            // Movimiento aéreo con límite horizontal
            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);

            // Settle: requerir varios frames grounded seguidos
            if (Motor.IsGrounded)
            {
                _groundedFrames++;

                if (_groundedFrames * dt >= Model.FallSettleTime)
                {
                    // Aterrizaje
                    _anim?.TriggerLand();
                    _anim?.SetGrounded(true);
                    _anim?.SetWalking(false);

                    // Frenar horizontal al aterrizar
                    var v = Motor.Velocity; v.x = 0f; v.z = 0f;
                    Motor.SetVelocity(v);

                    RequestTransition?.Invoke(ToWalkIdle);
                }
            }
            else
            {
                _groundedFrames = 0;
            }
        }

        /// <summary>Permite doble salto mientras estás en Fall si queda stock.</summary>
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } &&
                values[0] is string cmd &&
                cmd == CommandKeys.Jump &&
                values[1] is bool pressed && pressed)
            {
                if (Model.JumpsLeft > 0 || Model.HasExtraJump)
                {
                    RequestTransition?.Invoke(ToJumpAir);
                }

            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Actualiza MoveInputWorld a partir de RawMoveInput y la cámara.</summary>
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
