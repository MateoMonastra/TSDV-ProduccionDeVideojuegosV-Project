using FSM;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class Attack : State
    {
        private Transform _enemy;
        private Transform _player;
        private NavMeshAgent _agent;
        private System.Action _onAttackFinished;

        private EnemyModel model;
        private float _attackTimer = 0f;

        public Attack(Transform enemy, Transform player, NavMeshAgent agent, System.Action onAttackFinished, EnemyModel model)
        {
            this._enemy = enemy;
            this._player = player;
            this._agent = agent;
            this._onAttackFinished = onAttackFinished;
            this.model = model;
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

            if (_attackTimer >= model.AttackDuration)
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
