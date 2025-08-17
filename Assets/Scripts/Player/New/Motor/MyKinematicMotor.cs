using System;
using UnityEditor;
using UnityEngine;

namespace Player.New
{
    public class MyKinematicMotor : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private CapsuleCollider _capsule;

        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private LayerMask _groundMask;

        [Header("Movement Settings")] [SerializeField]
        private float _characterMass = 1f;

        [SerializeField] private float _maxSnapSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float gravity = 25f;

        [Header("Rotation Settings")] [SerializeField]
        private float _rotationSharpness = 10f;

        private Quaternion _targetRotation;
        private Quaternion _smoothedRotation;

        [Header("Ground Detection")] [SerializeField]
        private float _groundSnapDistance = 0.3f;

        [SerializeField] private float _groundedOffset = 0.1f;
        [SerializeField] private float _fallDetectionMultiplier = 2f;
        [SerializeField] private float _minGroundDotForSnap = 0.85f;
        
        [SerializeField] private float _ungroundTimeAfterJump = 0.1f;
        private float _ungroundTimer;

        private MovementSolver _movementSolver;
        private GroundingSolver _groundingSolver;
        private RigidbodyInteractionHandler _rigidbodyHandler;

        private Vector3 _velocity;
        private Vector3 _position;
        private Quaternion _rotation;
        private CharacterGroundingReport _groundingReport;
        private bool _wasGrounded;

        public Vector3 Velocity => _velocity;
        public bool IsGrounded => _ungroundTimer <= 0f
                                  && _groundingReport.IsStableOnGround
                                  && _groundingReport.FoundAnyGround;
        public Quaternion SmoothedRotation
        {
            get => _smoothedRotation;
            set => _smoothedRotation = value;
        }
        public void SetVelocity(Vector3 velocity) => _velocity = velocity;
        public void AddVelocity(Vector3 deltaVelocity) => _velocity += deltaVelocity;
        public void SetRotation(Quaternion rotation) => _rotation = rotation;
        public Vector3 CharacterUp => Vector3.up;
        public CharacterGroundingReport GroundingReport => _groundingReport;
        public void SetInputDirection(Vector2 moveInput, float moveSpeed)
        {
            Vector3 planarVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            _velocity.x = planarVelocity.x;
            _velocity.z = planarVelocity.z;
        }
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            return Vector3.ProjectOnPlane(direction, surfaceNormal).normalized;
        }
        private void Awake()
        {
            _rigidbodyHandler = new RigidbodyInteractionHandler(_characterMass);
            _movementSolver = new MovementSolver(_capsule, _collisionMask, _rigidbodyHandler);
            _groundingSolver = new GroundingSolver(_capsule, _groundMask);

            _position = transform.position;
            _rotation = transform.rotation;

            _targetRotation = transform.rotation;
            _smoothedRotation = transform.rotation;
        }
        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            
            _ungroundTimer = Mathf.Max(0f, _ungroundTimer - deltaTime);
            
            ApplyGravity(gravity, deltaTime);
            
            if (_ungroundTimer <= 0f)
            {
                float preMoveProbeDistance = _groundSnapDistance + Mathf.Max(0, -_velocity.y * deltaTime);
                _groundingSolver.CheckProbe(ref _position, _rotation, preMoveProbeDistance, _velocity, ref _groundingReport);
            }
            else
            {
                // Limpio el grounding report para que no cuente como grounded
                _groundingReport = default;
            }
            
            _movementSolver.Solve(ref _velocity, deltaTime, ref _position);

            transform.SetPositionAndRotation(_position, _rotation);
        }
        public void SetRotation(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                _rotation = Quaternion.LookRotation(direction);
            }
        }
        public void SmoothRotation(Vector3 direction, float sharpness, float deltaTime)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                _rotation = Quaternion.Slerp(_rotation, targetRot, 1 - Mathf.Exp(-sharpness * deltaTime));
            }
        }
        public void ApplyGravity(float gravity, float deltaTime)
        {
            if (!IsGrounded)
            {
                _velocity.y += gravity * deltaTime;
            }
        }
        
        /// <summary>Ignora detección de suelo durante "duration" segundos.</summary>
        public void ForceUnground(float duration)
        {
            _ungroundTimer = Mathf.Max(_ungroundTimer, duration);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_position,0.1f);
        }
    }
}