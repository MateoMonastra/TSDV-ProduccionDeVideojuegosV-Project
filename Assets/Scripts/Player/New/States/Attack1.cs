using System;
using FSM;
using Player.New.Audio;
using Player.New.VFX;

namespace Player.New
{
    public class Attack1 : AttackBase
    {
        public const string ToAttack2 = "ToAttack2";
        public const string ToIdle = "ToIdle";

        // Timing Configuration (These would ideally come from your Model/ScriptableObject)
        private float _hitTime = 0.2f; // When the damage happens
        private float _chainWindowStart = 0.2f; // When we start listening for the next combo
        private float _chainWindowEnd = 0.9f; // When the natural window closes (Duration)
        private float _lateGraceEnd = 1.0f; // Total time allowed to "save" the combo
        private float _totalDuration = 1.45f; // Total time allowed to "save" the combo

        private bool _hitProcessed;
        private bool _chainRequested;

        private readonly PlayerAnimationController _anim;
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;

        public Attack1(MyKinematicMotor m, PlayerModel mdl, Action<string> req,
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
            _chainRequested = false;
            ChainBuffered = false;

            // Trigger Visuals/Audio
            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack1();
            _vfxController?.Play(VfxEvent.BaseAttack);
            _audioController?.PlayPlayerAttack1();
        }

        public override void Tick(float dt)
        {
            t += dt;

            // 1. HIT LOGIC: Independent of other windows
            if (!_hitProcessed && t >= _hitTime)
            {
                _hitProcessed = true;
                TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);
            }

            if (t >= _chainWindowEnd && ChainBuffered)
            {
                ExecuteChain();
            }

            // 3. EXPIRATION LOGIC: If we pass the absolute last chance
            if (t >= _totalDuration)
            {
                _anim.SetCombatActive(false);
                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 1 } && values[0] is string cmd && cmd == CommandKeys.AttackPressed)
            {
                // If we are in the active window, chain immediately
                if (t >= _chainWindowStart && t <= _chainWindowEnd)
                {
                    BufferChain();
                }
                else if (t >= _chainWindowEnd && t <= _lateGraceEnd)
                {
                    ExecuteChain();
                }
            }
        }

        private void ExecuteChain()
        {
            Req?.Invoke(ToAttack2);
            Finish();
        }
    }
}