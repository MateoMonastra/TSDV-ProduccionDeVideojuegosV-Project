﻿using UnityEngine;
using System.Collections;

namespace Enemies.RangeEnemy.States
{
    public class SpecialAttack : RangedEnemyState
    {
        private Coroutine _specialAttackCoroutine;
        private bool _isAttacking;
        private GameObject _groundMark;
        private GameObject _bullet;
        private Vector3 _shootPoint;
        private System.Action _onFinishSpecialAttack;

        public SpecialAttack(Transform enemy, Transform player, RangedEnemyModel model,
            System.Action onFinishSpecialAttack
            , GameObject groundMark, GameObject bullet, Vector3 shootPoint) : base(enemy, player, model)
        {
            _groundMark = groundMark;
            _bullet = bullet;
            _onFinishSpecialAttack = onFinishSpecialAttack;
            _shootPoint = shootPoint;
        }

        public override void Enter()
        {
            base.Enter();
            _isAttacking = false;
            Attack();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
        }

        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            base.Exit();
        }

        private void Attack()
        {
            if (_isAttacking) return;
            _isAttacking = true;
            _specialAttackCoroutine = enemy.GetComponent<MonoBehaviour>().StartCoroutine(ExecuteSpecialAttack());
        }

        private IEnumerator ExecuteSpecialAttack()
        {
            Vector3[] targetPositions = new Vector3[model.AttacksCount];

            for (int i = 0; i < model.AttacksCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * model.AttackAreaRadius;
                Vector3 randomPosition = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
                Ray ray = new Ray(randomPosition, Vector3.down);

                if (Physics.Raycast(ray, out var hit, model.MaxRayDistance))
                {
                    targetPositions[i] = hit.point;

                    GameObject marker = GameObject.Instantiate(_groundMark, hit.point, Quaternion.identity);
                    GameObject.Destroy(marker, model.ProjectileFallTime + 0.5f);
                }
                //the default option was taken out by manuel petition 
                // else
                // {
                //     //else default
                //     Vector3 fallbackPosition = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
                //     targetPositions[i] = fallbackPosition;
                //
                //     GameObject marker = GameObject.Instantiate(_groundMark, fallbackPosition, Quaternion.identity);
                //     GameObject.Destroy(marker, model.ProjectileFallTime + 0.5f);
                // }
            }

            for (int i = 0; i < model.AttacksCount; i++)
            {
                GameObject projectile =
                    GameObject.Instantiate(_bullet, _shootPoint + Vector3.up * 2f, Quaternion.identity);

                Rigidbody rb = projectile.GetComponent<Rigidbody>();

                if (rb)
                {
                    Vector3 velocity = CalculateProjectileVelocity(_shootPoint + Vector3.up * 2f, targetPositions[i],
                        model.ProjectileFallTime);
                    rb.linearVelocity = velocity;
                }
            }

            yield return new WaitForSeconds(model.SpecialAttackCooldown);

            _isAttacking = false;
            _onFinishSpecialAttack?.Invoke();
        }

        private Vector3 CalculateProjectileVelocity(Vector3 start, Vector3 end, float timeToTarget)
        {
            Vector3 toTarget = end - start;
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);

            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;

            float t = timeToTarget;
            float vY = (y / t) + (0.5f * Mathf.Abs(Physics.gravity.y) * t);
            float vXZ = xz / t;

            Vector3 result = toTargetXZ.normalized * vXZ;
            result.y = vY;

            return result;
        }
    }
}