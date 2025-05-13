using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class Idle : BaseEnemyState
    {
        private System.Action _onEnterChase;
        public Idle(Transform enemy, Transform player, BaseEnemyModel model, System.Action onEnterChase) : base(enemy, player, model)
        {
            this._onEnterChase = onEnterChase;
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
                _onEnterChase?.Invoke();
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
