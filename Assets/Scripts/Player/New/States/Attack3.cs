namespace Player.New
{
    public class Attack3 : AttackBase
    {
        public const string ToIdle = "Attack3->AttackIdle";

        private readonly PlayerAnimationController _anim;
        public Attack3(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            Duration = Model.attack3Duration; // <--- FIX
            _anim?.TriggerAttack3();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            TryDoHitFrontal(0.5f, 55f);

            if (t >= Duration)
            {
                Model.attackComboOnCooldown = true;
                Model.attackComboCooldownLeft = Model.attackComboCooldown;

                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_AttackHit -= OnAnimHit;
        }
        private void OnAnimHit() => TryDoHitFrontal(0f);
    }
}
