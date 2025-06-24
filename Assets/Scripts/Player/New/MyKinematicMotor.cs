using System;
using UnityEngine;

namespace Player.New
{
    public class MyKinematicMotor : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _characterMass = 1f;
        [SerializeField] private float _groundSnapDistance = 0.1f;
        [SerializeField] private float _maxSnapSpeed = 5f;
        [SerializeField] private float _groundedOffset = 0.05f;

        private MovementSolver _movementSolver;
        private GroundingSolver _groundingSolver;
        private RigidbodyInteractionHandler _rigidbodyHandler;

        private Vector3 _velocity;
        private Vector3 _position;
        private Quaternion _rotation;
        private CharacterGroundingReport _groundingReport;
        private bool _wasGrounded;

        public Vector3 Velocity => _velocity;
        public bool IsGrounded => _groundingReport.IsStableOnGround && _groundingReport.FoundAnyGround;

        public void SetVelocity(Vector3 velocity) => _velocity = velocity;
        public void AddVelocity(Vector3 deltaVelocity) => _velocity += deltaVelocity;
        public void SetRotation(Quaternion rotation) => _rotation = rotation;

        public void SetInputDirection(Vector2 moveInput, float moveSpeed)
        {
            Vector3 planarVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            _velocity.x = planarVelocity.x;
            _velocity.z = planarVelocity.z;
        }

        private void Awake()
        {
            _rigidbodyHandler = new RigidbodyInteractionHandler(_characterMass);
            _movementSolver = new MovementSolver(_capsule, _collisionMask, _rigidbodyHandler);
            _groundingSolver = new GroundingSolver(_capsule, _groundMask);

            _position = transform.position;
            _rotation = transform.rotation;
        }

        private void Update()
        {
            transform.SetPositionAndRotation(_position, _rotation);
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;


            ApplyGravity(Physics.gravity.y, deltaTime);


            _movementSolver.Solve(ref _velocity, deltaTime, ref _position);

            // Calcular distancia de sondeo dinámica basada en velocidad
            float probeDistance = Mathf.Max(_groundSnapDistance, Mathf.Abs(_velocity.y * deltaTime) + _groundedOffset);
            _groundingSolver.CheckProbe(ref _position, _rotation, probeDistance, _velocity, ref _groundingReport);

            // Snap to ground si estamos cerca y moviéndonos hacia abajo
            if (_groundingReport.FoundAnyGround && !_groundingReport.SnappingPrevented &&
                _velocity.y <= 0 && _groundingReport.GroundPoint.y >= (_position.y - probeDistance))
            {
                _position.y = _groundingReport.GroundPoint.y;
                _velocity.y = 0;
            }

            transform.SetPositionAndRotation(_position, _rotation);
            _wasGrounded = IsGrounded;
        }

        public void ApplyGravity(float gravity, float deltaTime)
        {
            if (!IsGrounded)
            {
                _velocity.y += gravity * deltaTime;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (_groundingSolver != null && _capsule != null)
            {
                _groundingSolver.DrawGizmos(transform.position, transform.rotation);
            }
        }
    }
}