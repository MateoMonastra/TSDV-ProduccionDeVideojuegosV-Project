using FSM;
using UnityEngine;

namespace Player.New
{
    public class JumpAir : LocomotionState
    {
        public const string ToFall = "JumpAir->Fall";
        private readonly float _airDetectDelay;
        private float _t;
        private readonly PlayerAnimationController _anim;

        public JumpAir(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
            float airDetectDelay = 0.02f, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req) { _airDetectDelay = airDetectDelay; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;

            Motor.ForceUnground(0.10f);

            if (Model.jumpsLeft > 0) Model.jumpsLeft--;

            var v = Motor.Velocity;
            if (v.y < 0f) v.y = 0f;

            // ⬇️ APLICA MULTIPLICADOR DE SALTO TAMBIÉN EN DOBLE SALTO
            float jumpV = Model.jumpSpeed * Mathf.Max(0.01f, Model.actionJumpSpeedMultiplier);
            v.y += jumpV;

            Motor.SetVelocity(v);

            _anim?.TriggerDoubleJump();
            _anim?.SetGrounded(false);
            _anim?.SetFalling(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // mantener MoveInputWorld actualizado
            Vector3 up = Motor.CharacterUp;
            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < 1e-4f) camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camRight = Vector3.Cross(up, camFwd);
            Model.moveInputWorld = camFwd * Model.rawMoveInput.y + camRight * Model.rawMoveInput.x;
            if (Model.moveInputWorld.sqrMagnitude > 1e-6f) Model.moveInputWorld.Normalize();

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.airHorizontalSpeed);

            if (_t >= _airDetectDelay && !Motor.IsGrounded)
                RequestTransition?.Invoke(ToFall);
        }
    }
}
