using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Locomoción en suelo: idle + caminar. Resuelve input en espacio mundo,
    /// coyote jump y el armado/hold para iniciar Sprint.
    /// </summary>
    public class WalkIdle : LocomotionState
    {
        // ──────────────────────────────────────────────────────────────────────
        public const string ToJump   = "ToJump";
        public const string ToFall   = "ToFall";
        public const string ToSprint = "ToSprint";
        // ──────────────────────────────────────────────────────────────────────

        private readonly System.Action<bool> _onWalk;
        private readonly float _coyoteTime;
        private float _timeSinceUngrounded;
        private readonly PlayerAnimationController _anim;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WalkIdle(MyKinematicMotor m,
                        PlayerModel mdl,
                        Transform cam,
                        System.Action<string> requestTransition,
                        System.Action<bool> onWalk = null,
                        float coyoteTime = 0.12f,
                        PlayerAnimationController anim = null)
            : base(m, mdl, cam, requestTransition)
        {
            _onWalk = onWalk;
            _coyoteTime = Mathf.Max(0f, coyoteTime);
            _anim = anim;
        }

        /// <summary>Entra al estado: resetea coyote y flags de anim.</summary>
        public override void Enter()
        {
            base.Enter();
            _timeSinceUngrounded = 0f;
            _anim?.SetGrounded(true);
            _anim?.SetFalling(false);
            _anim?.SetWalking(false);
        }

        /// <summary>Sale del estado: apaga eventos/animación de caminar.</summary>
        public override void Exit()
        {
            base.Exit();
            _onWalk?.Invoke(false);
            _anim?.SetWalking(false);
        }

        /// <summary>
        /// Tick por frame: input → mundo, coyote/fall, locomoción y ventana de sprint.
        /// </summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);

            UpdateMoveInputWorld();
            
            if (HandleCoyoteFall(dt))
                return;
            
            ApplyLocomotion(dt, inAir: false);

            bool walking = ComputeIsWalking();
            _onWalk?.Invoke(walking);
            _anim?.SetWalking(walking);
            
            if (UpdateSprintWindowAndMaybeStart(dt, walking, Motor.IsGrounded))
                return;
        }

        /// <summary>
        /// Entrada de comandos (evita strings mágicos usando CommandKeys).
        /// </summary>
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == CommandKeys.Jump)
            {
                bool pressed = (bool)values[1];
                if (pressed && (Motor.IsGrounded || _timeSinceUngrounded <= _coyoteTime) && Model.JumpsLeft > 0)
                {
                    RequestTransition?.Invoke(ToJump);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        #region Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Convierte el input local (WASD/arrows) a un vector de mundo según la cámara.</summary>
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
        /// Maneja coyote time y dispara caída cuando corresponde.
        /// Devuelve true si transicionó a <see cref="ToFall"/>.
        /// </summary>
        private bool HandleCoyoteFall(float dt)
        {
            if (Motor.IsGrounded)
            {
                _timeSinceUngrounded = 0f;
                _anim?.SetGrounded(true);
                _anim?.SetFalling(false);
                return false;
            }

            _timeSinceUngrounded += dt;
            if (_timeSinceUngrounded > _coyoteTime)
            {
                RequestTransition?.Invoke(ToFall);
                return true;
            }

            return false;
        }

        /// <summary>Detecta si estamos caminando (velocidad horizontal + input válido).</summary>
        private bool ComputeIsWalking()
        {
            Vector3 hv = Motor.Velocity;
            hv.y = 0f;
            return hv.sqrMagnitude > 0.0001f && Model.MoveInputWorld.sqrMagnitude > 0f;
        }

        /// <summary>
        /// Gestiona la ventana y el hold para iniciar Sprint.
        /// Devuelve true si transicionó a <see cref="ToSprint"/>.
        /// </summary>
        private bool UpdateSprintWindowAndMaybeStart(float dt, bool walking, bool grounded)
        {
            if (!Model.SprintArmed)
                return false;

            Model.SprintArmTimeLeft -= dt;
            if (Model.SprintArmTimeLeft <= 0f)
            {
                Model.SprintArmed = false;
                Model.SprintHoldCounter = 0f;
                return false;
            }

            // hold acumulado solo cuando el botón está mantenido
            Model.SprintHoldCounter = Model.DashHeld ? (Model.SprintHoldCounter + dt) : 0f;

            if (Model.SprintHoldCounter >= Model.SprintHoldTime && walking && grounded)
            {
                // cerrar ventana antes de salir
                Model.SprintArmed = false;
                Model.SprintHoldCounter = 0f;

                RequestTransition?.Invoke(ToSprint);
                return true;
            }

            return false;
        }

        #endregion
    }
}
