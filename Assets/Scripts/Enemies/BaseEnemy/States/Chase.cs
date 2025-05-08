using UnityEngine;
using UnityEngine.AI;

namespace Enemies.BaseEnemy.States
{
    public class Chase : BaseEnemyState
    {
        private NavMeshAgent _agent;
        private System.Action _onExitChase;
        private System.Action _onEnterAttack;

        public Chase(Transform enemy, Transform player, BaseEnemyModel model, NavMeshAgent agent,
            System.Action onExitChase, System.Action onEnterAttack) : base(enemy, player, model)
        {
            this._agent = agent;
            this._onExitChase = onExitChase;
            this._onEnterAttack = onEnterAttack;
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

            _agent.SetDestination(player.position);
        }

        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            _agent.ResetPath();
        }

        private bool IsPlayerInChaseRange(float distance)
        {
            if (distance > model.OuterRadius)
            {
                _agent.ResetPath();
                _onExitChase?.Invoke();
                return false;
            }

            return true;
        }

        private bool IsPlayerInAttackRange(float distance)
        {
            if (distance <= model.AttackRange)
            {
                _agent.ResetPath();
                _onEnterAttack?.Invoke();
                return true;
            }

            return false;
        }
    }
}