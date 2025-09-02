namespace Player.New
{
    /// <summary>Tercer golpe del combo. Cierra y aplica cooldown del combo.</summary>
    public class Attack3 : AttackBase
    {
        public const string ToIdle = "ToIdle";

        private readonly PlayerAnimationController _anim;

        public Attack3(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            Duration = Model.Attack3Duration;  // ← duración específica

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack3();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit;
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

            TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);

            if (t >= Duration)
            {
                Model.AttackComboOnCooldown   = true;
                Model.AttackComboCooldownLeft = Model.AttackComboCooldown;

                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        private void OnAnimHit() => TryDoHitFrontal(0f);
    }
}