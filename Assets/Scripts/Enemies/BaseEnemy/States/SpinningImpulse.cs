using System;
using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class SpinningImpulse : BaseEnemyState
    {
        private UnityEngine.AI.NavMeshAgent _agent;
        private Rigidbody _rigidbody;
        private TrailRenderer _trailRenderer;
        private Action _onImpulseStarted;
        private Action _onImpulseEnded;
        private Vector3 _impulseSource;
        private readonly RaycastHit[] _hit = new RaycastHit[1];
        private Vector2 _impulseForce;
        private float elapsed = 0;
        
        public SpinningImpulse(Transform enemy, Transform player, TrailRenderer trailRenderer, BaseEnemyModel model, UnityEngine.AI.NavMeshAgent agent,
            Rigidbody rigidbody, Action onImpulseStarted, Action onImpulseEnded) : base(enemy, player, model)
        {
            _agent = agent;
            _rigidbody = rigidbody;
            _onImpulseStarted = onImpulseStarted;
            _onImpulseEnded = onImpulseEnded;
            _trailRenderer = trailRenderer;
        }

        public override void Enter()
        {
            base.Enter();

            _onImpulseStarted?.Invoke();
            _rigidbody.isKinematic = false;
            _agent.enabled = false;
            Vector3 fromPlayer = player.forward;
            fromPlayer.y = 0;
            Vector3 toPlayer = _impulseSource;
            toPlayer.y = enemy.position.y;
            enemy.LookAt(toPlayer);

            elapsed = 0;
            
            _trailRenderer.enabled = true;

            _rigidbody.AddForce(
                (enemy.position - _impulseSource).normalized * _impulseForce.x + Vector3.up * _impulseForce.y,
                ForceMode.Impulse);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            elapsed += delta;
            if (_rigidbody.linearVelocity.y < 0)
            {
                _rigidbody.linearVelocity += Vector3.up * (Physics.gravity.y * (model.SpinningFallMultiplier - 1) * delta);
            }
            else if (_rigidbody.linearVelocity.y > 0)
            {
                _rigidbody.linearVelocity += Vector3.up * (Physics.gravity.y * (model.SpinningLowJumpMultiplier - 1) * delta);
            }

            Quaternion deltaRotation = Quaternion.Euler(-460 * delta, 0, 0);
        
            _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
            
            GroundCheck();
        }

        private void GroundCheck()
        {
            Debug.DrawRay(enemy.position + enemy.up * 1.2f,Vector3.down * 2.2f, Color.yellow);
            bool isGrounded = Physics.Raycast(enemy.position + enemy.up * 1.2f, Vector3.down, 2.2f, model.GroundLayer);

            if (isGrounded && elapsed > 0.65f)  //Enough time to get off the ground
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
            _trailRenderer.enabled = false;

            _agent.ResetPath();
        }

        public void SetImpulse(Vector2 newImpulse)
        {
            _impulseForce = newImpulse;
        }

        public void SetImpulseSource(Vector3 impulseSource)
        {
            _impulseSource = impulseSource;
        }
    }
}