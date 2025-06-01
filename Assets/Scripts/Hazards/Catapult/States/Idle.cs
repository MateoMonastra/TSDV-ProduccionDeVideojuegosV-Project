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

            if (distance <= _model.AttackRange)
            {
                    _onEnterAttackRange?.Invoke();
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