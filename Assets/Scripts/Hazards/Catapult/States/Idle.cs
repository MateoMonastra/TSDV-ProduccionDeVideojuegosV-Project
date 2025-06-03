using FSM;
using UnityEngine;

namespace Hazards.Catapult.States
{
    public class Idle : State
    {
        private System.Action _onEnterAttackRange;
        private Transform _target;
        private Transform _enemy;
        private CatapultModel _model;
        private float _cooldownTimer = 0f;
        private bool _isInCooldown = false;
        public Idle(Transform enemy, Transform target, CatapultModel model, System.Action onEnterAttackRange)
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
            float distance = Vector3.Distance(_enemy.position, _target.position);

            if (_isInCooldown)
            {
                _cooldownTimer += delta;
                
                if (!(_cooldownTimer >= _model.CooldownBetweenAttacks)) return;
                
                _isInCooldown = false;
                _cooldownTimer = 0f;
                return;
            }

            if (!(distance <= _model.AttackRange)) return;
            _onEnterAttackRange?.Invoke();
            _isInCooldown = true;
            _cooldownTimer = 0f;
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