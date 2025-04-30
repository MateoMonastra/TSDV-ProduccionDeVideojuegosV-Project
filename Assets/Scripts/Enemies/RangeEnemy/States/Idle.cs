using Enemy;
using UnityEngine;

namespace Enemies.RangeEnemy.States
{
    public class Idle : BaseState
    {
        private System.Action _onEnterAttackRange;
        private System.Action _specialAttackAction;
        private int _attacksCount = 0;
        private RangedEnemyModel _model;
        private float _cooldownTimer = 0f;
        private bool _isInCooldown = false;
        public Idle(Transform enemy, Transform player, BaseEnemyModel baseModel,RangedEnemyModel model, System.Action onEnterAttackRange
            , System.Action specialAttackAction) : base(enemy, player, baseModel)
        {
            this._onEnterAttackRange = onEnterAttackRange;
            this._specialAttackAction = specialAttackAction;
            _model = model;
        }
        
        public override void Enter()
        {
            base.Enter();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            float distance = Vector3.Distance(enemy.position, player.position);

            if (_isInCooldown)
            {
                _cooldownTimer += delta;
                if (_cooldownTimer >= _model.CooldownBetweenAttacks)
                {
                    _isInCooldown = false;
                    _cooldownTimer = 0f;
                }
                return;
            }

            if (distance <= model.AttackRange)
            {
                if (_attacksCount < _model.AttacksCountToSpecialAttack)
                {
                    _onEnterAttackRange?.Invoke();
                    _attacksCount++;
                    _isInCooldown = true;
                    _cooldownTimer = 0f;
                    Debug.Log($"Ataque normal {_attacksCount}");
                }
                else
                {
                    _specialAttackAction?.Invoke();
                    _attacksCount = 0;
                    _isInCooldown = true;
                    _cooldownTimer = 0f;
                    Debug.Log("Ataque especial");
                }
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