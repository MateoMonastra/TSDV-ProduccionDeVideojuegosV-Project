using System;
using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class SpinningVerticalImpulse : BaseEnemyState
    {
        private UnityEngine.AI.NavMeshAgent _agent;
        private Rigidbody _rigidbody;
        private TrailRenderer _trailRenderer;
        private Action _onImpulseStarted;
        private Action _onImpulseEnded;
        private Action _onGetUp;
        private Vector3 _impulseSource;
        private readonly RaycastHit[] _hit = new RaycastHit[1];
        private Vector2 _impulseForce;
        private float elapsed = 0;
        private float recoveryElapsed = 0;
        private bool _isGrounded;

        public SpinningVerticalImpulse(Transform enemy, Transform player, TrailRenderer trailRenderer,
            BaseEnemyModel model, UnityEngine.AI.NavMeshAgent agent,
            Rigidbody rigidbody, Action onImpulseStarted, Action onImpulseEnded, Action onGetUp) : base(enemy, player,
            model)
        {
            _agent = agent;
            _rigidbody = rigidbody;
            _onImpulseStarted = onImpulseStarted;
            _onImpulseEnded = onImpulseEnded;
            _onGetUp = onGetUp;
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
            recoveryElapsed = 0;
            _isGrounded = false;

            _trailRenderer.enabled = true;

            _rigidbody.AddForce(
                (enemy.position - _impulseSource).normalized * _impulseForce.x + Vector3.up * _impulseForce.y,
                ForceMode.Impulse);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            

            if (!_isGrounded)
            {
                elapsed += delta;
                if (_rigidbody.linearVelocity.y < 0)
                {
                    _rigidbody.linearVelocity +=
                        Vector3.up * (Physics.gravity.y * (model.SpinningFallMultiplier - 1) * delta);
                }
                else if (_rigidbody.linearVelocity.y > 0)
                {
                    _rigidbody.linearVelocity +=
                        Vector3.up * (Physics.gravity.y * (model.SpinningLowJumpMultiplier - 1) * delta);
                }

                Quaternion deltaRotation = Quaternion.Euler(-460 * delta, 0, 0);

                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
                
                GroundCheck();
            }
            else
            {
                recoveryElapsed += delta;
                if (recoveryElapsed > 1.6f)
                {
                    _onGetUp?.Invoke();
                }
            }
        }

        private void GroundCheck()
        {
            Ray rayo = new Ray(enemy.position + enemy.up * 1.6f, Vector3.down * 1.4f);
            Debug.DrawRay(rayo.origin, rayo.direction, Color.yellow);
            bool isGrounded = Physics.Raycast(rayo, out RaycastHit hit,1.4f,model.GroundLayer);

            if (isGrounded && elapsed > 0.65f) //Enough time to get off the ground
            {
                _onImpulseEnded?.Invoke();
                
                //_agent.enabled = true;
                enemy.position = hit.point;
                _rigidbody.isKinematic = true;
                _trailRenderer.enabled = false;


                _rigidbody.rotation = Quaternion.identity;
                //_agent.ResetPath();
                Vector3 toPlayer = _impulseSource;
                toPlayer.y = enemy.position.y;
                enemy.LookAt(toPlayer);
                

                _isGrounded = true;
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


            _rigidbody.rotation = Quaternion.identity;
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