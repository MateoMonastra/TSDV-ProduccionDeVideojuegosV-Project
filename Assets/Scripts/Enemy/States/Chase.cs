using FSM;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class Chase : State
    {
        private Transform enemy;
        private Transform player;
        private NavMeshAgent agent;
        private float outerRadius;
        private float attackRange;
        private System.Action onExitChase;
        private System.Action onEnterAttack;

        public Chase(Transform enemy, Transform player, NavMeshAgent agent, float outerRadius, float attackRange,
            System.Action onExitChase, System.Action onEnterAttack)
        {
            this.enemy = enemy;
            this.player = player;
            this.agent = agent;
            this.outerRadius = outerRadius;
            this.attackRange = attackRange;
            this.onExitChase = onExitChase;
            this.onEnterAttack = onEnterAttack;
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            float distance = Vector3.Distance(enemy.position, player.position);

            if (!IsPlayerInChaseRange(distance)) return;

            if (IsPlayerInAttackRange(distance)) return;

            agent.SetDestination(player.position);
        }
        
        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            agent.ResetPath();
        }

        private bool IsPlayerInChaseRange(float distance)
        {
            if (distance > outerRadius)
            {
                agent.ResetPath();
                onExitChase?.Invoke();
                return false;
            }

            return true;
        }
        private bool IsPlayerInAttackRange(float distance)
        {
            if (distance <= attackRange)
            {
                agent.ResetPath();
                onEnterAttack?.Invoke();
                return true;
            }

            return false;
        }
    }
}
