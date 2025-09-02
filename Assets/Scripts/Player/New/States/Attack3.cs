namespace Player.New
{
    /// <summary>Tercer golpe del combo. Termina el combo y aplica cooldown.</summary>
    public class Attack3 : AttackBase
    {
        public const string ToIdle = "ToIdle";

        private readonly PlayerAnimationController _anim;

        public Attack3(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            Duration = Model.Attack3Duration;

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack3();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);

            if (t >= Duration)
            {
                // aplicar cooldown de combo
                Model.AttackComboOnCooldown = true;
                Model.AttackComboCooldownLeft = Model.AttackComboCooldown;

                Req?.Invoke(ToIdle);
                Finish();
            }
        }
    }
}