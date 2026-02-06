using System;
using FSM;
using Player.New.Audio;
using Player.New.VFX;

namespace Player.New
{
    /// <summary>Segundo golpe del combo. Buffer + late-grace para encadenar a A3.</summary>
    public class Attack2 : AttackBase
    {
        public const string ToAttack3 = "ToAttack3";
        public const string ToIdle = "ToIdle";

        private bool _hitProcessed;
        private readonly PlayerAnimationController _anim;
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;

        public Attack2(MyKinematicMotor m, PlayerModel mdl, Action<string> req,
            PlayerAnimationController anim = null, PlayerVfxController vfxController = null,
            PlayerAudioController audioController = null)
            : base(m, mdl, req)
        {
            _vfxController = vfxController;
            _anim = anim;
            _audioController = audioController;
        }

        public override void Enter()
        {
            base.Enter();
            if (!M.IsGrounded)
            {
                _anim?.SetCombatActive(false);
                Req?.Invoke(ToIdle);
                Finish();
                return;
            }

            // Initialize local state
            t = 0;
            _hitProcessed = false;
            ChainBuffered = false;

            // Trigger Visuals/Audio
            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack2();
            _vfxController?.Play(VfxEvent.Attack2);
            _audioController?.PlayPlayerAttack2();

            knockbackDistance = Model.Attack2KnockbackDistance;
            stunDuration = Model.Attack2StunDuration;
        }

        public override void Tick(float dt)
        {
            t += dt;

            // 1. HIT LOGIC
            if (t >= Model.Attack2HitTime && t <= Model.Attack1ChainWindowEnd)
            {
                TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);
            }

            // 2. BUFFERED TRANSITION: If player pressed early, transition at the end of the window
            if (t >= Model.Attack2ChainWindowEnd && ChainBuffered)
            {
                ExecuteChain();
            }

            // 3. EXPIRATION LOGIC
            if (t >= Model.Attack2TotalDuration)
            {
                _anim?.SetCombatActive(false);
                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 1 } && values[0] is string cmd && cmd == CommandKeys.AttackPressed)
            {
                // Buffer period: Input is saved for later
                if (t >= Model.Attack2ChainWindowStart && t < Model.Attack2ChainWindowEnd)
                {
                    BufferChain();
                }
                // Grace period: Input triggers next attack immediately
                else if (t >= Model.Attack2ChainWindowEnd && t <= Model.Attack2LateGraceEnd)
                {
                    ExecuteChain();
                }
            }
        }

        private void ExecuteChain()
        {
            Req?.Invoke(ToAttack3);
            Finish();
        }
    }
}