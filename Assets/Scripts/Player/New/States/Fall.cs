using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Caída en aire: permite control horizontal y vuelve a Idle tras
    /// un pequeño “settle” grounded. También permite doble salto en caída.
    /// </summary>
    public class Fall : LocomotionState
    {
        public const string ToWalkIdle = "ToWalkIdle";
        public const string ToJumpAir  = "ToJumpAir";
        
        private float _groundedTimer;

        private readonly PlayerAnimationController _anim;

        public Fall(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _groundedTimer = 0f;
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

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);

            if (Motor.IsGrounded)
            {
                _groundedTimer += dt;
                if (_groundedTimer >= Model.FallSettleTime)
                    RequestTransition?.Invoke(ToWalkIdle);
            }
            else
            {
                _groundedTimer = 0f;
            }
        }

        /// <summary>Permite doble salto durante la caída si queda stock.</summary>
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == CommandKeys.Jump)
            {
                bool pressed = (bool)values[1];
                if (pressed && Model.JumpsLeft > 0)
                {
                    RequestTransition?.Invoke(ToJumpAir);
                }
            }
        }
    }
}
