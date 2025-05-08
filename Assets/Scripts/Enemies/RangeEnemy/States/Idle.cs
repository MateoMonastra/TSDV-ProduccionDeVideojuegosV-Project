using UnityEngine;

namespace Enemies.RangeEnemy.States
{
    public class Idle : RangedEnemyState
    {
        private System.Action _onEnterAttackRange;
        private System.Action _specialAttackAction;
        private int _attacksCount = 0;
        private float _cooldownTimer = 0f;
        private bool _isInCooldown = false;
        public Idle(Transform enemy, Transform player, RangedEnemyModel model, System.Action onEnterAttackRange
            , System.Action specialAttackAction) : base(enemy, player, model)
        {
            this._onEnterAttackRange = onEnterAttackRange;
            this._specialAttackAction = specialAttackAction;
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
                if (_cooldownTimer >= model.CooldownBetweenAttacks)
                {
                    _isInCooldown = false;
                    _cooldownTimer = 0f;
                }
                return;
            }

            if (distance <= model.AttackRange)
            {
                if (_attacksCount < model.AttacksCountToSpecialAttack)
                {
                    _onEnterAttackRange?.Invoke();
                    _attacksCount++;
                    _isInCooldown = true;
                    _cooldownTimer = 0f;
                }
                else
                {
                    _specialAttackAction?.Invoke();
                    _attacksCount = 0;
                    _isInCooldown = true;
                    _cooldownTimer = 0f;
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