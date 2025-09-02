using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Doble salto en aire. Aplica nuevo impulso vertical y controla movimiento en aire.
    /// Al comenzar a descender (tras breve delay) transiciona a <see cref="Fall"/>.
    /// </summary>
    public class JumpAir : LocomotionState
    {
        public const string ToFall = "ToFall";
        
        private float _t;
        private readonly PlayerAnimationController _anim;

        public JumpAir(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, cam, req)
        { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            
            if (Model.JumpsLeft > 0)
            {
                Model.JumpsLeft = Mathf.Max(0, Model.JumpsLeft - 1);
            }
            else if (Model.HasExtraJump)
            {
                Model.HasExtraJump = false;
            }
            else
            {
                RequestTransition?.Invoke(ToFall);
                return;
            }

            var v = Motor.Velocity;
            v.y = Model.JumpSpeed * Mathf.Max(0.01f, Model.ActionJumpSpeedMultiplier);
            Motor.SetVelocity(v);

            _anim?.TriggerDoubleJump();
        }


        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);

            if (_t >= Model.JumpAirAirDetectDelay && Motor.Velocity.y <= 0f)
            {
                RequestTransition?.Invoke(ToFall);
            }
        }
    }
}