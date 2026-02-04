using Player.New.Audio;
using Player.New.VFX;
using Player.Old;

namespace Player.New
{
    /// <summary>Tercer golpe del combo. Cierra y aplica cooldown del combo.</summary>
    public class Attack3 : AttackBase
    {
        public const string ToIdle = "ToIdle";

        private float _windUpTime = 0.45f;
        private readonly PlayerAnimationController _anim;
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;

        public Attack3(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req,
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
            Duration = Model.Attack3Duration;

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack3();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit;
            _vfxController?.Play(VfxEvent.Attack3);
            _audioController.PlayPlayerAttack3();
            knockbackDistance = Model.Attack3KnockbackDistance;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_AttackHit -= OnAnimHit;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            if (t >= _windUpTime)
                TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);

            if (t >= Duration)
            {
                Model.AttackComboOnCooldown = true;
                Model.AttackComboCooldownLeft = Model.AttackComboCooldown;

                _anim.SetCombatActive(false);
                Req?.Invoke(ToIdle);
                _anim.SetCombatActive(false);
                Finish();
            }
        }

        private void OnAnimHit() => TryDoHitFrontal(0f);
    }
}