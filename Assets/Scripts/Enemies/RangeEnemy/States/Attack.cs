using UnityEngine;

namespace Enemies.RangeEnemy.States
{
    public class Attack : BaseState
    {
        private GameObject _projectilePrefab;
        private Transform _shootPoint;
        private RangedEnemyModel _model;
        private System.Action _onFinishAttack;
        private int _shotsFired;
        private float _shotTimer;
        private float _cooldownTimer;
        private bool _inCooldown;
        private int _shotSeries;

        public Attack(Transform enemy, Transform player, BaseEnemyModel baseModel, RangedEnemyModel model, GameObject projectilePrefab,
            Transform shootPoint,System.Action onFinishAttack)
            : base(enemy, player, baseModel)
        {
            this._projectilePrefab = projectilePrefab;
            this._shootPoint = shootPoint;
            this._model = model;
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
                if (_cooldownTimer >= _model.CooldownBetweenShots)
                {
                    _inCooldown = false;
                    _shotsFired = 0;
                    _cooldownTimer = 0f;
                }

                return;
            }

            _shotTimer += delta;
            if (_shotTimer >= _model.TimeBetweenShots && _shotsFired < _model.TotalShots)
            {
                Shoot();
                _shotsFired++;
                _shotTimer = 0f;
            }

            if (_shotsFired >= _model.TotalShots && !_inCooldown)
            {
                _inCooldown = true;
                _cooldownTimer = 0f;
                _shotSeries++;
            }

            if (_model.ShotsSeries == _shotSeries)
            {
                _onFinishAttack?.Invoke();
                
                Debug.Log($"termino, shotSeries {_shotSeries}, model.ShotsSeries {_model.ShotsSeries}");
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
            Vector3 direction = (player.position - _shootPoint.position).normalized;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * _model.ShotSpeed;
            }
        }
        
        private void ResetStartValues()
        {
            _shotsFired = 0;
            _shotTimer = 0f;
            _cooldownTimer = 0f;
            _shotSeries = 0;
            _inCooldown = false;
            Debug.Log($"reset start values, shotSeries {_shotSeries}, model.ShotsSeries {_model.ShotsSeries}");
        }
    }
}