namespace Enemies.BaseEnemy.States
{
    public sealed class Idle : BaseEnemyState
    {
        private readonly System.Action _toChase;

        public Idle(EnemyContext ctx, System.Action toChase) : base(ctx)
        {
            _toChase = toChase;
        }

        public override void Enter()
        {
            Ctx.Anims?.SetWalkAnimation(false);
            if (Ctx.Agent && Ctx.Agent.enabled)
            {
                Ctx.Agent.ResetPath();
                Ctx.Agent.isStopped = true;
            }
        }

        public override void Tick(float dt)
        {
            if (SqrDistanceToPlayer() <= Ctx.SqrInnerRadius)
                _toChase?.Invoke();
        }
    }
}