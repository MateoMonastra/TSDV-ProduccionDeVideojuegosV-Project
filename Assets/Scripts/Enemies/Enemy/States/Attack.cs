using Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies.Enemy.States
{
    public class Attack : BaseEnemyState
    {
        private NavMeshAgent _agent;
        private System.Action _onAttackFinished;

        private float _attackDuration;
        private float _attackTimer = 0f;

        public Attack(Transform enemy, Transform player, BaseEnemyModel model, NavMeshAgent agent, System.Action onAttackFinished) : base(enemy, player, model)
        {
            this._agent = agent;
            this._onAttackFinished = onAttackFinished;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0f;
            _agent.ResetPath();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            _attackTimer += delta;

            if (_attackTimer >= _attackDuration)
            {
                _onAttackFinished?.Invoke();
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
