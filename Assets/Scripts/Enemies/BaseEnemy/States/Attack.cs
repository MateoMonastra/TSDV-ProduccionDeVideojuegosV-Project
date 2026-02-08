using UnityEngine;
using UnityEngine.AI;

namespace Enemies.BaseEnemy.States
{
    public class Attack : BaseEnemyState
    {
        private NavMeshAgent _agent;
        private Collider _collider;
        private EnemyAnimationController _anim;
        private System.Action _onAttackFinished;
        private System.Action _onAttackDelay;
        private System.Action _onAttackHit;

        private float _attackDuration;
        private float _attackTimer = 0f;
        private bool _delayed;

        public Attack(Transform enemy, Transform player, BaseEnemyModel model, NavMeshAgent agent, Collider collider, EnemyAnimationController anim,
            System.Action onAttackDelay, System.Action onAttackHit,
            System.Action onAttackFinished) : base(enemy, player, model)
        {
            this._agent = agent;
            this._onAttackFinished = onAttackFinished;
            this._onAttackDelay = onAttackDelay;
            this._onAttackHit = onAttackHit;
            _collider = collider;
            _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0f;
            _delayed = false;
            _agent.ResetPath();
            _onAttackDelay?.Invoke();
            if (_anim != null) _anim.OnAnim_AttackDamage += OnAttackDamageEvent;
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            _attackTimer += delta;


            if (model.AttackDelay <= _attackTimer && !_delayed)
            {
                _delayed = true;
                _attackTimer = 0;
                _onAttackHit?.Invoke();
            }
            else
            {
                Vector3 flatTarget = player.position;
                flatTarget.y = enemy.position.y;
                enemy.LookAt(flatTarget, Vector3.up);

            }

            if (!_delayed) return;

            if (_attackTimer >= model.AttackDuration)
            {
                _collider.gameObject.SetActive(false);
                _onAttackFinished?.Invoke();
            }
        }

        private void OnAttackDamageEvent()
        {
            _collider.gameObject.SetActive(true);
        }

        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            _collider.gameObject.SetActive(false);
            if (_anim != null) _anim.OnAnim_AttackDamage -= OnAttackDamageEvent;
            base.Exit();
        }
    }
}