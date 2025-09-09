using FSM;
using UnityEngine;

namespace Hazards.Cannon.States
{
    public class Idle : State
    {
        private System.Action _onEnterAttackRange;
        private Transform _target;
        private Transform _enemy;
        private CannonModel _model;
        private float _cooldownTimer = 0f;
        private bool _isInCooldown = false;
        private bool _needsUpdate;
        public Idle(Transform enemy, Transform target, CannonModel model, System.Action onEnterAttackRange)
        {
            _onEnterAttackRange = onEnterAttackRange;
            _enemy = enemy;
            _target = target;
            _model = model;
        }

        public override void Enter()
        {
            base.Enter();
        }
        
        public override void Tick(float delta)
        {
            base.Tick(delta);

            Vector3 flatDifference = _target.position - _enemy.position;
            flatDifference.y = 0f;
            float distance = flatDifference.magnitude;

            if (_isInCooldown)
            {
                _cooldownTimer += delta;

                if (!(_cooldownTimer >= _model.CooldownBetweenAttacks)) return;

                _isInCooldown = false;
                _cooldownTimer = 0f;
                return;
            }

            if (!(distance <= _model.AttackRange)) return;

            if (!IsFacingTarget())
            {
                Debug.Log(_enemy.gameObject.name + " is facing not target");
                _needsUpdate = true;
                return;
            }

            _onEnterAttackRange?.Invoke();
            _isInCooldown = true;
            _cooldownTimer = 0f;
        }


        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);

            if (_needsUpdate)
                UpdateRotation(delta);
        }

        private void UpdateRotation(float delta)
        {
            Vector3 directionToPlayer = _target.position - _enemy.position;
            directionToPlayer.y = 0f;

            if (directionToPlayer == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            _enemy.rotation = Quaternion.RotateTowards(
                _enemy.rotation,
                targetRotation,
                _model.RotateVelocity * delta
            );

            _needsUpdate = false;
        }

        public override void Exit()
        {
            base.Exit();
        }
        
        private bool IsFacingTarget()
        {
            float threshold  = 0.95f;
    
            Vector3 directionToPlayer = (_target.position - _enemy.position).normalized;
            directionToPlayer.y = 0f;

            Vector3 forward = _enemy.forward;
            forward.y = 0f; 

            float dot = Vector3.Dot(forward.normalized, directionToPlayer);

            return dot > threshold; 
        }

    }
}