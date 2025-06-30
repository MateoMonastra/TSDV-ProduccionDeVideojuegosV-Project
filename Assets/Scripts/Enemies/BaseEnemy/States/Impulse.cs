using System;
using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class Impulse : BaseEnemyState
    {
        private UnityEngine.AI.NavMeshAgent _agent;
        private Rigidbody _rigidbody;
        private Action _onImpulseStarted;
        private Action _onImpulseEnded;
        private float _impulseTimer = 0;

        public Impulse(Transform enemy, Transform player, BaseEnemyModel model, UnityEngine.AI.NavMeshAgent agent,
            Rigidbody rigidbody, Action onImpulseStarted, Action onImpulseEnded) : base(enemy, player, model)
        {
            _agent = agent;
            _rigidbody = rigidbody;
            _onImpulseStarted = onImpulseStarted;
            _onImpulseEnded = onImpulseEnded;
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("impulsing");
            _impulseTimer = 0;
            _rigidbody.isKinematic = false;
            _agent.enabled = false;
            Vector3 fromPlayer = enemy.position - player.position;
            fromPlayer.y = 4;

            _rigidbody.AddForce(fromPlayer.normalized * model.ImpulseForce, ForceMode.Impulse);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (_impulseTimer < model.DamagedStunTime)
            {
                _impulseTimer += delta;
            }
            else
            {
                _onImpulseEnded?.Invoke();
            }
        }

        private void GroundCheck()
        {
            Physics.RaycastNonAlloc(player.position);
        }

        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            _agent.enabled = true;
            _rigidbody.isKinematic = true;
            _agent.ResetPath();
        }
    }
}