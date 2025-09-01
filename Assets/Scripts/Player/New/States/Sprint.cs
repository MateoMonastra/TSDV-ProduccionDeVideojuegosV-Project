using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Estado de sprint: corre indefinidamente a MoveSpeed * SprintSpeedMultiplier.
    /// Entra si WalkIdle lo solicita tras cumplir la ventana/hold post-Dash.
    /// Sale si: no hay movimiento suficiente, se pierde suelo, o alguna acción bloquea locomoción.
    /// Permite saltar (corta el sprint y transiciona a JumpGround).
    /// </summary>
    public class Sprint : LocomotionState
    {
        // ──────────────────────────────────────────────────────────────────────
        public const string ToWalkIdle = "ToWalkIdle";
        public const string ToFall     = "ToFall";
        public const string ToJump     = "ToJump";
        // ──────────────────────────────────────────────────────────────────────

        private readonly PlayerAnimationController _anim;

        public Sprint(MyKinematicMotor m,
                      PlayerModel mdl,
                      Transform cam,
                      System.Action<string> requestTransition,
                      PlayerAnimationController anim = null)
            : base(m, mdl, cam, requestTransition)
        {
            _anim = anim;
        }

        /// <summary>Al entrar: aplica multiplicador de sprint y enciende anim de movimiento.</summary>
        public override void Enter()
        {
            base.Enter();
            Model.ActionMoveSpeedMultiplier = Model.SprintSpeedMultiplier;
            _anim?.SetWalking(true); // Lugarcito para poner la animacion de Run
        }

        /// <summary>Al salir: limpia multiplicadores y desarma el sprint.</summary>
        public override void Exit()
        {
            base.Exit();
            Model.ActionMoveSpeedMultiplier = 1f;
            Model.SprintArmed = false;
            _anim?.SetWalking(false);
        }

        /// <summary>
        /// Tick por frame: input→mundo, locomoción, checks de movimiento/suelo/bloqueos.
        /// </summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);

            // Si otra mecánica bloquea locomoción, terminamos sprint.
            if (Model.LocomotionBlocked)
            {
                RequestTransition?.Invoke(ToWalkIdle);
                return;
            }

            UpdateMoveInputWorld();

            // Locomoción en suelo (stop instantáneo si no hay input)
            ApplyLocomotion(dt, inAir: false);

            if (!IsMovingEnough())
            {
                RequestTransition?.Invoke(ToWalkIdle);
                return;
            }

            if (!Motor.IsGrounded)
            {
                RequestTransition?.Invoke(ToFall);
                return;
            }
        }

        /// <summary>
        /// Permite salto durante sprint: corta el sprint y transiciona a JumpGround.
        /// </summary>
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } &&
                values[0] is string cmd &&
                cmd == CommandKeys.Jump &&
                values[1] is bool pressed &&
                pressed &&
                Motor.IsGrounded &&
                Model.JumpsLeft > 0)
            {
                RequestTransition?.Invoke(ToJump);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        #region Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Convierte el input 2D a dirección de mundo según la cámara.</summary>
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

        /// <summary>
        /// Comprueba si la magnitud horizontal de la velocidad actual supera el mínimo
        /// para mantener el sprint.
        /// </summary>
        private bool IsMovingEnough()
        {
            Vector3 hv = Motor.Velocity;
            hv.y = 0f;
            float min = Model.SprintMinSpeedToKeep;
            return hv.sqrMagnitude > (min * min);
        }

        #endregion
    }
}
