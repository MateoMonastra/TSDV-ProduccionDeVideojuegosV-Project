using UnityEngine;
using UnityEngine.AI;

namespace Enemies.Enemy.States
{
    public class Attack : BaseEnemyState
    {
        private NavMeshAgent _agent;
        private Collider _collider;
        private System.Action _onAttackFinished;

        private float _attackDuration;
        private float _attackTimer = 0f;

        public Attack(Transform enemy, Transform player, BaseEnemyModel model, NavMeshAgent agent, Collider collider,
            System.Action onAttackFinished) : base(enemy, player, model)
        {
            this._agent = agent;
            this._onAttackFinished = onAttackFinished;
            _collider = collider;
        }

        public override void Enter()
        {
            base.Enter();
            _collider.gameObject.SetActive(true);
            _attackTimer = 0f;
            _agent.ResetPath();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            _attackTimer += delta;
            
            if (_attackTimer >= model.AttackDuration)
            {
                _collider.gameObject.SetActive(false);
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