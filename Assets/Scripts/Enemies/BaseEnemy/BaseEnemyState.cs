using UnityEngine;

namespace Enemies.BaseEnemy
{
    /// <summary>Base de estados con utilidades comunes.</summary>
    public abstract class BaseEnemyState : FSM.State
    {
        protected readonly EnemyContext Ctx;
        protected BaseEnemyState(EnemyContext ctx) => Ctx = ctx;
        
        protected float SqrDistanceToPlayer()
        {
            Vector3 d = Ctx.Target.position - Ctx.Self.position;
            d.y = 0f;
            return d.sqrMagnitude;
        }

        protected void ToggleAgent(bool on)
        {
            if (!Ctx.Agent) return;
            Ctx.Agent.enabled = on;
        }

        protected void ToggleHitBox(bool on)
        {
            if (!Ctx.HitBox) return;
            Ctx.HitBox.enabled = on;
        }
    }
}