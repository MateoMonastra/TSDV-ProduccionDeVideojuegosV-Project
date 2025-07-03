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
        private float _fallMultiplier = 2.5f;
        private float _lowJumpMultiplier = 2f;
        private readonly RaycastHit[] _hit = new RaycastHit[1];

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

            _onImpulseStarted?.Invoke();
            _rigidbody.isKinematic = false;
            _agent.enabled = false;
            Vector3 fromPlayer = player.forward;
            fromPlayer.y = 0;
            Vector3 toPlayer = player.position;
            toPlayer.y = enemy.position.y;
            enemy.LookAt(toPlayer);

            _rigidbody.AddForce(
                fromPlayer.normalized * model.HorizontalImpulseForce + Vector3.up * model.VerticalImpulseForce,
                ForceMode.Impulse);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (_rigidbody.linearVelocity.y < 0)
            {
                _rigidbody.linearVelocity += Vector3.up * (Physics.gravity.y * (model.FallMultiplier - 1) * delta);
            }
            else if (_rigidbody.linearVelocity.y > 0)
            {
                _rigidbody.linearVelocity += Vector3.up * (Physics.gravity.y * (model.LowJumpMultiplier - 1) * delta);
            }
            
            GroundCheck();
        }

        private void GroundCheck()
        {
            bool isGrounded = Physics.Raycast(enemy.position + Vector3.up * 0.5f, -enemy.up, 0.5f,
                1 << LayerMask.NameToLayer("Default"));

            if (isGrounded)
            {
                _onImpulseEnded?.Invoke();
            }
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