using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.New
{
    public class MyKinematicMotor : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private CapsuleCollider capsule;

        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private LayerMask groundMask;

        [Header("Movement Settings")] [SerializeField]
        private float characterMass = 1f;

        [SerializeField] private float maxSnapSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = 25f;

        [Header("Rotation Settings")] [SerializeField]
        private float rotationSharpness = 10f;

        private Quaternion _targetRotation;
        private Quaternion _smoothedRotation;

        [Header("Ground Detection")] [SerializeField]
        private float groundSnapDistance = 0.3f;

        [SerializeField] private float groundedOffset = 0.1f;
        [SerializeField] private float fallDetectionMultiplier = 2f;
        [SerializeField] private float minGroundDotForSnap = 0.85f;

        [SerializeField] private float ungroundTimeAfterJump = 0.1f;
        private float _ungroundTimer;

        [Header("Triggers")] [SerializeField] private LayerMask triggersMask;
        [SerializeField] private int maxPickupsPerFrame = 8;

        [SerializeField, Tooltip("Publicar pose con RB kinemático para que Unity dispare OnTrigger/OnCollision")]
        private bool useRigidbodyForPose = true;

        [Header("Runtime Locks")]
        [SerializeField, Tooltip("Si está activo, el motor NO integra ni mueve al personaje.")]
        private bool frozen = false;

        /// <summary>Congela/descongela la integración del motor.</summary>
        public bool Frozen
        {
            get => frozen;
            set
            {
                frozen = value;
                if (frozen)
                {
                    // Asegurar que la pose congelada sea la actual
                    _velocity = Vector3.zero;
                    _position = transform.position;
                    _rotation = transform.rotation;
                }
            }
        }

        private Rigidbody _rb;
        private readonly Collider[] _pickupHits = new Collider[16];

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
            _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
                _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            _rigidbodyHandler = new RigidbodyInteractionHandler(characterMass);
            _movementSolver = new MovementSolver(capsule, collisionMask | groundMask, _rigidbodyHandler);
            _groundingSolver = new GroundingSolver(capsule, groundMask);

            _position = transform.position;
            _rotation = transform.rotation;

            _targetRotation = transform.rotation;
            _smoothedRotation = transform.rotation;
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            if (frozen)
            {
                _velocity = Vector3.zero;
                transform.SetPositionAndRotation(_position, _rotation);
                return;
            }

            _ungroundTimer = Mathf.Max(0f, _ungroundTimer - deltaTime);

            ApplyGravity(gravity, deltaTime);

            if (_ungroundTimer <= 0f)
            {
                float preMoveProbeDistance = groundSnapDistance + Mathf.Max(0, -_velocity.y * deltaTime);
                _groundingSolver.CheckProbe(ref _position, _rotation, preMoveProbeDistance, _velocity,
                    ref _groundingReport);

                if (_velocity.y <= maxSnapSpeed)
                    TrySnapToGround(groundSnapDistance);
            }
            else
            {
                _groundingReport = default;
            }

            _movementSolver.Solve(ref _velocity, deltaTime, ref _position);

            if (_ungroundTimer <= 0f && _velocity.y <= maxSnapSpeed)
                TrySnapToGround(groundSnapDistance);


            if (useRigidbodyForPose && _rb != null)
            {
                _rb.MovePosition(_position);
                _rb.MoveRotation(_rotation);
            }
            else
            {
                transform.SetPositionAndRotation(_position, _rotation);
                Physics.SyncTransforms();
            }
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

        // === Helpers locales del motor (mismos cálculos que usa MovementSolver) ===
        private Vector3 GetCapsuleBottomAt(Vector3 pos)
        {
            return pos + capsule.center + Vector3.down * (capsule.height * 0.5f - capsule.radius);
        }

        private Vector3 GetCapsuleTopAt(Vector3 pos)
        {
            return pos + capsule.center + Vector3.up * (capsule.height * 0.5f - capsule.radius);
        }

        /// <summary>Snap vertical suave al suelo estable si estamos cerca.</summary>
        private bool TrySnapToGround(float maxSnapDist)
        {
            if (_groundingReport.SnappingPrevented || !_groundingReport.IsStableOnGround ||
                !_groundingReport.FoundAnyGround)
                return false;

            float upDot = Vector3.Dot(_groundingReport.GroundNormal, Vector3.up);
            if (upDot < minGroundDotForSnap)
                return false;

            Vector3 bottom = GetCapsuleBottomAt(_position);

            float desiredBottomY = _groundingReport.GroundPoint.y + capsule.radius + groundedOffset;
            float deltaY = desiredBottomY - bottom.y;

            if (deltaY >= -maxSnapDist && deltaY <= maxSnapDist)
            {
                _position.y += deltaY;

                if (_velocity.y < 0f) _velocity.y = 0f;
                return true;
            }

            return false;
        }

        public void WarpTo(Vector3 position, Quaternion rotation)
        {
            _position = position;
            _rotation = rotation;
            _velocity = Vector3.zero;

            transform.SetPositionAndRotation(_position, _rotation);
            Physics.SyncTransforms();
        }

        // Aplica el movimiento de una plataforma sin resetear la velocidad del jugador.
        // - deltaPosition / deltaRotation: delta de la plataforma en este FixedUpdate
        // - rotateVelocity: si true, también rota la velocidad del jugador con la plataforma
        public void ApplyPlatformMotion(Vector3 deltaPosition, Quaternion deltaRotation, bool rotateVelocity = true)
        {
            _position += deltaPosition;
            _rotation = deltaRotation * _rotation;


            if (rotateVelocity)
                _velocity = deltaRotation * _velocity;


            transform.SetPositionAndRotation(_position, _rotation);
            Physics.SyncTransforms();
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_position, 0.1f);
        }
    }
}