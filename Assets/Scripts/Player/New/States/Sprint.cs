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
        public const string ToWalkIdle = "Sprint->WalkIdle";
        public const string ToFall     = "Sprint->Fall";
        public const string ToJump     = "Sprint->JumpGround";

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

        public override void Enter()
        {
            base.Enter();
            
            Model.ActionMoveSpeedMultiplier = Model.SprintSpeedMultiplier;
            
            _anim?.SetWalking(true);
        }

        public override void Exit()
        {
            base.Exit();
            Model.ActionMoveSpeedMultiplier = 1f;
            Model.SprintArmed = false;
            _anim?.SetWalking(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            
            if (Model.LocomotionBlocked)
            {
                RequestTransition?.Invoke(ToWalkIdle);
                return;
            }

            UpdateMoveInputWorld();
            ApplyLocomotion(dt, inAir: false);
            
            var hv = Motor.Velocity; hv.y = 0f;
            bool moving = hv.sqrMagnitude > Model.SprintMinSpeedToKeep * Model.SprintMinSpeedToKeep;
            if (!moving)
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
        

        private void UpdateMoveInputWorld()
        {
            Vector3 up = Motor.CharacterUp;

            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;

            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.MoveInputWorld = camFwd * Model.RawMoveInput.y + camRight * Model.RawMoveInput.x;
            if (Model.MoveInputWorld.sqrMagnitude > 1e-6f) Model.MoveInputWorld = Model.MoveInputWorld.normalized;
        }
    }
}
