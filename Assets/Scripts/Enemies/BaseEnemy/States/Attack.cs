namespace Enemies.BaseEnemy.States
{
    /// <summary>
    /// Ataque por ventana (delay → hit window → fin).
    /// Idealmente abrí/cerrá la ventana con Animation Events; esto soporta ambas formas.
    /// </summary>
    public sealed class Attack : BaseEnemyState
    {
        private readonly System.Action _toChase;
        private float _timer;
        private bool _hitWindowOpen;

        public Attack(EnemyContext ctx, System.Action toChase) : base(ctx)
        {
            _toChase = toChase;
        }

        public override void Enter()
        {
            _timer = 0f;
            _hitWindowOpen = false;
            Ctx.Agent?.ResetPath();
            Ctx.Anims?.SetAttackAnimation();
            ToggleHitBox(false);
        }

        public override void Tick(float dt)
        {
            _timer += dt;
            
            if (!_hitWindowOpen && _timer >= Ctx.Model.AttackDelay)
            {
                _hitWindowOpen = true;
                Ctx.Anims?.SetAttackHitAnimation();
                ToggleHitBox(true);
            }
            
            if (_hitWindowOpen && _timer >= Ctx.Model.AttackDelay + Ctx.Model.AttackDuration)
            {
                ToggleHitBox(false);
                _toChase?.Invoke();
            }
        }

        public override void Exit()
        {
            ToggleHitBox(false);
        }
        
        public void OpenHitWindow()  => ToggleHitBox(true);
        public void CloseHitWindow() => ToggleHitBox(false);
    }
}