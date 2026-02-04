using System;
using FSM;
using Player.New.Audio;
using Player.New.VFX;
using UnityEngine;

namespace Player.New
{
    /// <summary>Tercer golpe del combo. Cierra y aplica cooldown del combo.</summary>
    public class Attack3 : AttackBase
    {
        public const string ToIdle = "ToIdle";

        private bool _hitProcessed;
        private readonly PlayerAnimationController _anim;
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;

        public Attack3(MyKinematicMotor m, PlayerModel mdl, Action<string> req,
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
            
            // Safety check for air-attacks if your system doesn't support them
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

            // Trigger Visuals/Audio
            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack3();
            _vfxController?.Play(VfxEvent.Attack3);
            _audioController?.PlayPlayerAttack3();

            knockbackDistance = Model.Attack3KnockbackDistance;
        }

        public override void Tick(float dt)
        {
            t += dt;

            // 1. HIT LOGIC
            if (!_hitProcessed && t >= Model.Attack3HitTime)
            {
                _hitProcessed = true;
                TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);
            }

            // 2. FINISHER / COOLDOWN LOGIC
            if (t >= Model.Attack3TotalDuration)
            {
                // Set combo cooldown values in the model
                Model.AttackComboOnCooldown = true;
                Model.AttackComboCooldownLeft = Model.AttackComboCooldown;

                _anim?.SetCombatActive(false);
                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            Debug.Log("Exited 3");
        }
    }
}