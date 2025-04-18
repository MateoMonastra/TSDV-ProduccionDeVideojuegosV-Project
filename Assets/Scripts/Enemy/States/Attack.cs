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

        private float _attackDuration;
        private float _attackTimer = 0f;

        public Attack(Transform enemy, Transform player, NavMeshAgent agent, System.Action onAttackFinished, float attackDuration)
        {
            this._enemy = enemy;
            this._player = player;
            this._agent = agent;
            this._onAttackFinished = onAttackFinished;
            this._attackDuration = attackDuration;
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
