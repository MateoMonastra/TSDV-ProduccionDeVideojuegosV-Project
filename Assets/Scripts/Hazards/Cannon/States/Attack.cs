using System;
using FSM;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hazards.Cannon.States
{
    public class Attack : State
    {
        private Transform _shootPoint;
        private GameObject _projectilePrefab;
        private GameObject _groundMarkPrefab;
        private Transform _target;
        private CannonModel _model;
        private Action _onAttackComplete;
        private float _elapsedTime;

        public Attack(Transform shootPoint, GameObject projectilePrefab, GameObject groundMarkPrefab, Transform target,
            CannonModel model, Action onAttackComplete)
        {
            _shootPoint = shootPoint;
            _projectilePrefab = projectilePrefab;
            _groundMarkPrefab = groundMarkPrefab;
            _target = target;
            _model = model;
            _onAttackComplete = onAttackComplete;
        }

        public override void Enter()
        {
            base.Enter();
            StartAttack();
            _elapsedTime = 0f;
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
            _elapsedTime += delta;

            if (_elapsedTime >= _model.FlightDuration)
            {
                _onAttackComplete?.Invoke();
            }
        }


        private void StartAttack()
        {
            Vector3 launchPosition = _shootPoint.position;
            Vector3 targetPosition = _target.position;

            Vector3 velocity = CalculateParabolicVelocity(launchPosition, targetPosition, _model.FlightDuration);

            if (_model.DebugDrawTrajectory)
                DrawTrajectoryGizmo(launchPosition, velocity, _model.FlightDuration);

            GameObject projectile =
                UnityEngine.Object.Instantiate(_projectilePrefab, launchPosition, Quaternion.identity);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = velocity;

            Ray ray = new Ray(_target.position, Vector3.down);

            if (Physics.Raycast(ray, out var hit, _model.MaxRayDistance))
            {
                GameObject marker = Object.Instantiate(_groundMarkPrefab, hit.point, Quaternion.identity);
                Object.Destroy(marker, _model.FlightDuration + 0.5f);
            }

            _onAttackComplete?.Invoke();
        }

        private Vector3 CalculateParabolicVelocity(Vector3 startPoint, Vector3 endPoint, float duration)
        {
            float gravity = Mathf.Abs(Physics.gravity.y);
            Vector3 displacement = endPoint - startPoint;

            Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
            Vector3 velocityXZ = displacementXZ / duration;

            float velocityY = (displacement.y + 0.5f * gravity * duration * duration) / duration;

            return velocityXZ + Vector3.up * velocityY;
        }

        private void DrawTrajectoryGizmo(Vector3 startPosition, Vector3 initialVelocity, float duration)
        {
            float timestep = duration / _model.DrawResolution;
            Vector3 prevPoint = startPosition;

            for (int i = 1; i <= _model.DrawResolution; i++)
            {
                float t = i * timestep;
                Vector3 point = startPosition + initialVelocity * t + Physics.gravity * (0.5f * t * t);
                Debug.DrawLine(prevPoint, point, Color.magenta, 2f);
                prevPoint = point;
            }
        }
    }
}