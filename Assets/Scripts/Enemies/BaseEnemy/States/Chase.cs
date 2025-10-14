using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public sealed class Chase : BaseEnemyState
    {
        private readonly System.Action _toIdle, _toAttack;

        public Chase(EnemyContext ctx, System.Action toIdle, System.Action toAttack) : base(ctx)
        {
            _toIdle   = toIdle;
            _toAttack = toAttack;
        }

        public override void Enter()
        {
            Ctx.Anims?.SetWalkAnimation(true);

            if (Ctx.Agent)
            {
                Ctx.Agent.enabled = true;
                Ctx.Agent.isStopped = false;
            
                Ctx.Agent.stoppingDistance = Mathf.Max(0.1f, Ctx.Model.AttackRange * 0.9f);
            }
        }

        public override void Tick(float dt)
        {
            float sqrDist = SqrDistanceToPlayer();

            if (sqrDist > Ctx.SqrOuterRadius)
            {
                Ctx.Agent?.ResetPath();
                _toIdle?.Invoke();
                return;
            }

            if (sqrDist <= Ctx.SqrAttackRange)
            {
                Ctx.Agent?.ResetPath();
                _toAttack?.Invoke();
                return;
            }

            if (Ctx.Agent && Ctx.Agent.enabled)
                Ctx.Agent.SetDestination(Ctx.Target.position);
        }

        public override void Exit()
        {
            Ctx.Anims?.SetWalkAnimation(false);
            Ctx.Agent?.ResetPath();
        }
    }
}