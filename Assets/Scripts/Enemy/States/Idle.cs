using FSM;
using UnityEngine;

namespace Enemy
{
    public class Idle : State
    {
        private Transform enemy;
        private Transform player;
        private float innerRadius;
        private System.Action onEnterChase;
        public Idle(Transform enemy, Transform player, float innerRadius, System.Action onEnterChase)
        {
            this.enemy = enemy;
            this.player = player;
            this.innerRadius = innerRadius;
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

            if (distance <= innerRadius)
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
