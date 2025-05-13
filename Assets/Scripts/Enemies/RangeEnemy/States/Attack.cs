using UnityEngine;

namespace Enemies.RangeEnemy.States
{
    public class Attack : RangedEnemyState
    {
        private GameObject _projectilePrefab;
        private Transform _shootPoint;
        private System.Action _onFinishAttack;
        private int _shotsFired;
        private float _shotTimer;
        private float _cooldownTimer;
        private bool _inCooldown;
        private int _shotSeries;

        public Attack(Transform enemy, Transform player, RangedEnemyModel model, GameObject projectilePrefab,
            Transform shootPoint,System.Action onFinishAttack) : base(enemy, player, model )
        {
            this._projectilePrefab = projectilePrefab;
            this._shootPoint = shootPoint;
            _onFinishAttack = onFinishAttack;
        }

        public override void Enter()
        {
            base.Enter();
            ResetStartValues();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (_inCooldown)
            {
                _cooldownTimer += delta;
                if (_cooldownTimer >= model.CooldownBetweenShots)
                {
                    _inCooldown = false;
                    _shotsFired = 0;
                    _cooldownTimer = 0f;
                }

                return;
            }

            _shotTimer += delta;
            if (_shotTimer >= model.TimeBetweenShots && _shotsFired < model.TotalShots)
            {
                Shoot();
                _shotsFired++;
                _shotTimer = 0f;
            }

            if (_shotsFired >= model.TotalShots && !_inCooldown)
            {
                _inCooldown = true;
                _cooldownTimer = 0f;
                _shotSeries++;
            }

            if (model.ShotsSeries == _shotSeries)
            {
                _onFinishAttack?.Invoke();
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

        private void Shoot()
        {
            GameObject projectile = GameObject.Instantiate(_projectilePrefab, _shootPoint.position, Quaternion.identity);
            Vector3 targetPosition = player.position + Vector3.up * model.Height;
            Vector3 direction = (targetPosition - _shootPoint.position).normalized;
           
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            
            if (rb)
            {
                rb.linearVelocity = direction * model.ShotSpeed;
            }
        }
        
        private void ResetStartValues()
        {
            _shotsFired = 0;
            _shotTimer = 0f;
            _cooldownTimer = 0f;
            _shotSeries = 0;
            _inCooldown = false;
        }
    }
}