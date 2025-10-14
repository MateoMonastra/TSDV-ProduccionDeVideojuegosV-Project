namespace Enemies.BaseEnemy.States
{
    /// <summary>
    /// Estado de muerte: limpia componentes y hace despawn tras el timer.
    /// </summary>
    public sealed class Death : FSM.State
    {
        private readonly EnemyContext _ctx;
        private float _t;

        public Death(EnemyContext ctx) => _ctx = ctx;

        public override void Enter()
        {
            _ctx.Anims?.SetDeathAnimation();
            if (_ctx.Agent)  _ctx.Agent.enabled = false;
            if (_ctx.HitBox) _ctx.HitBox.enabled = false;
            _t = 0f;
        }

        public override void Tick(float dt)
        {
            _t += dt;
            if (_t >= _ctx.Model.DeathTime)
            {
                _ctx.Self.gameObject.SetActive(false);
            }
        }
    }
}