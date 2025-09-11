using FSM;
using Player.New.Audio;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Primer salto desde el suelo. Aplica impulso vertical y, tras un pequeño delay
    /// de detección de aire, pasa a <see cref="Fall"/> cuando comienza a descender.
    /// Permite pedir el doble salto (→ <see cref="JumpAir"/>).
    /// </summary>
    public class JumpGround : LocomotionState
    {
        public const string ToFall    = "ToFall";
        public const string ToJumpAir = "ToJumpAir";
        
        private float _t;
        private readonly PlayerAnimationController _anim;
        private readonly PlayerAudioController _audioController;

        public JumpGround(MyKinematicMotor m, PlayerModel mdl, Transform cam, System.Action<string> req,
            PlayerAnimationController anim = null, PlayerAudioController audioController = null)
            : base(m, mdl, cam, req)
        {
            _anim = anim; 
            _audioController = audioController;
        }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            
            Model.JumpsLeft = Mathf.Max(0, Model.JumpsLeft - 1);
            var v = Motor.Velocity;
            v.y = Model.JumpSpeed * Mathf.Max(0.01f, Model.ActionJumpSpeedMultiplier);
            Motor.SetVelocity(v);
            
            Model.JumpWasPureVertical = Model.RawMoveInput.sqrMagnitude <= 1e-6f;

            _anim?.SetGrounded(false);
            _anim?.TriggerJump();
            
            _audioController?.PlayJumpAudio();
        }

        public override void Exit()
        {
            base.Exit();
            _anim?.SetFalling(true);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;
            
            ApplyLocomotion(dt, inAir: true, limitAirSpeed: true, maxAirSpeed: Model.AirHorizontalSpeed);
            
            if (_t >= Model.JumpGroundAirDetectDelay && Motor.Velocity.y <= 0f)
            {
                RequestTransition?.Invoke(ToFall);
            }
        }

        /// <summary>Permite doble salto si queda stock.</summary>
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 2 } && values[0] is string cmd && cmd == CommandKeys.Jump)
            {
                bool pressed = (bool)values[1];
                if (pressed && (Model.JumpsLeft > 0 || Model.HasExtraJump))
                {
                    RequestTransition?.Invoke(ToJumpAir);
                }

            }
        }
    }
}
