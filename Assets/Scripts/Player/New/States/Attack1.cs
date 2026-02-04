using System;
using FSM;
using Player.New.Audio;
using Player.New.VFX;
using UnityEngine;

namespace Player.New
{
    public class Attack1 : AttackBase
    {
        public const string ToAttack1 = "ToAttack1";
        public const string ToAttack2 = "ToAttack2";
        public const string ToIdle = "ToIdle";

        

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
            
            Debug.Log("Entered 1");
        }

        public override void Tick(float dt)
        {
            t += dt;

            // 1. HIT LOGIC: Independent of other windows
            if (!_hitProcessed && t >= Model.Attack1HitTime)
            {
                _hitProcessed = true;
                TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);
            }

            if (t >= Model.Attack1ChainWindowEnd && ChainBuffered)
            {
                ExecuteChain();
            }

            // 3. EXPIRATION LOGIC: If we pass the absolute last chance
            if (t >= Model.Attack1TotalDuration)
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
                if (t >= Model.Attack1ChainWindowStart && t <= Model.Attack1ChainWindowEnd)
                {
                    BufferChain();
                }
                else if (t >= Model.Attack1ChainWindowEnd && t <= Model.Attack1LateGraceEnd)
                {
                    ExecuteChain();
                }
                else if(t >= Model.Attack1LateGraceEnd && t <= Model.Attack1TotalDuration)
                {
                    Req?.Invoke(ToAttack1);
                    Finish();
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