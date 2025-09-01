using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Correr indefinido a MoveSpeed * SprintMultiplier.
    /// Entra si: sprintArmed (viene de terminar un Dash) + dashHeld + hay movimiento.
    /// Sale si: se corta el movimiento (idle) o se activa otra mecánica (locomotionBlocked / dash / jump / etc).
    /// </summary>
    public class Sprint : LocomotionState
    {
        public const string ToWalkIdle = "Sprint->WalkIdle";
        public const string ToFall     = "Sprint->Fall";
        public const string ToJump     = "Sprint->JumpGround";

        private readonly PlayerAnimationController _anim;

        public Sprint(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            // multiplicador de acción para reutilizar ApplyLocomotion sin tocarla
            Model.actionMoveSpeedMultiplier = Model.sprintSpeedMultiplier;
            // anim: usamos walking “true” (si tenés un bool/param de run, lo podemos llamar aquí)
            _anim?.SetWalking(true);
        }

        public override void Exit()
        {
            base.Exit();
            Model.actionMoveSpeedMultiplier = 1f;
            // al salir, desarmamos el sprint (se requiere nuevo dash)
            Model.sprintArmed = false;
            _anim?.SetWalking(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            // Si otra mecánica bloquea locomoción, salimos
            if (Model.locomotionBlocked)
            {
                RequestTransition?.Invoke(ToWalkIdle);
                return;
            }

            // Actualizar input world (igual que en WalkIdle)
            Vector3 up = Motor.CharacterUp;
            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.moveInputWorld = camFwd * Model.rawMoveInput.y + camRight * Model.rawMoveInput.x;
            if (Model.moveInputWorld.sqrMagnitude > 1e-6f) Model.moveInputWorld.Normalize();

            // Locomoción en suelo (stop instant si no hay input)
            ApplyLocomotion(dt, inAir: false);

            // Si nos quedamos sin movimiento, cortar sprint
            var hv = Motor.Velocity; hv.y = 0f;
            bool moving = hv.sqrMagnitude > Model.sprintMinSpeedToKeep * Model.sprintMinSpeedToKeep;
            if (!moving)
            {
                RequestTransition?.Invoke(ToWalkIdle);
                return;
            }

            // Caída inmediata si perdemos suelo
            if (!Motor.IsGrounded)
            {
                RequestTransition?.Invoke(ToFall);
                return;
            }
        }

        public override void HandleInput(params object[] values)
        {
            // Permitir salto: cortar sprint y saltar
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == "Jump" &&
                values[1] is bool pressed && pressed && Motor.IsGrounded && Model.jumpsLeft > 0)
            {
                RequestTransition?.Invoke(ToJump);
            }
        }
    }
}
