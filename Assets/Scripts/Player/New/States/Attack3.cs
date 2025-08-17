namespace Player.New
{
    public class Attack3 : AttackBase
    {
        public const string ToIdle = "Attack3->AttackIdle";

        public Attack3(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req) : base(m, mdl, req) { }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            Duration = Model.Attack3Duration;
            // TODO: anim/sfx A3
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            TryDoHitFrontal(0.5f, 55f);

            if (t >= Duration)
            {
                // Al terminar el 3er golpe, activar cooldown del combo
                Model.AttackComboOnCooldown = true;
                Model.AttackComboCooldownLeft = Model.AttackComboCooldown;

                Req?.Invoke(ToIdle);
                Finish();
            }
        }
    }
}