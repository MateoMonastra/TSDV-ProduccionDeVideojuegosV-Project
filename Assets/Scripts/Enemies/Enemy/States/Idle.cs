using Enemy;
using UnityEngine;

namespace Enemies.Enemy.States
{
    public class Idle : BaseState
    {
        private System.Action onEnterChase;
        public Idle(Transform enemy, Transform player, BaseEnemyModel model, System.Action onEnterChase) : base(enemy, player, model)
        {
            this.onEnterChase = onEnterChase;
        }
        
        public override void Enter()
        {
            base.Enter();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
            
            float distance = Vector3.Distance(enemy.position, player.position);

            if (distance <= model.InnerRadius)
            {
                onEnterChase?.Invoke();
            }
        }

        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
