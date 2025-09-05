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
        
        [Header("Pickups / Triggers")]
        [SerializeField] private LayerMask _pickupMask;
        [SerializeField] private int _maxPickupsPerFrame = 8;
        
        [SerializeField, Tooltip("Publicar pose con RB kinemático para que Unity dispare OnTrigger/OnCollision")]
        private bool useRigidbodyForPose = true;
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
                _rb.useGravity  = false;
                _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            
            _rigidbodyHandler = new RigidbodyInteractionHandler(_characterMass);
            _movementSolver = new MovementSolver(_capsule, _collisionMask | _groundMask, _rigidbodyHandler);
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
                
                if (_velocity.y <= _maxSnapSpeed)
                    TrySnapToGround(_groundSnapDistance);
            }
            else
            {
                _groundingReport = default;
            }

            _movementSolver.Solve(ref _velocity, deltaTime, ref _position);
            
            if (_ungroundTimer <= 0f && _velocity.y <= _maxSnapSpeed)
                TrySnapToGround(_groundSnapDistance);

            // DetectPickups();
            
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
            return pos + _capsule.center + Vector3.down * (_capsule.height * 0.5f - _capsule.radius);
        }

        private Vector3 GetCapsuleTopAt(Vector3 pos)
        {
            return pos + _capsule.center + Vector3.up * (_capsule.height * 0.5f - _capsule.radius);
        }

        /// <summary>Snap vertical suave al suelo estable si estamos cerca.</summary>
        private bool TrySnapToGround(float maxSnapDist)
        {
            if (_groundingReport.SnappingPrevented || !_groundingReport.IsStableOnGround || !_groundingReport.FoundAnyGround)
                return false;

            // Chequeo de ángulo (usamos tu threshold “para snap”)
            float upDot = Vector3.Dot(_groundingReport.GroundNormal, Vector3.up);
            if (upDot < _minGroundDotForSnap)
                return false;

            // Distancia vertical desde el bottom del capsule al plano de contacto deseado
            Vector3 bottom = GetCapsuleBottomAt(_position);
            // Queremos que el bottom quede a radius + _groundedOffset sobre el punto del suelo
            float desiredBottomY = _groundingReport.GroundPoint.y + _capsule.radius + _groundedOffset;
            float deltaY = desiredBottomY - bottom.y;

            if (deltaY >= -maxSnapDist && deltaY <= maxSnapDist)
            {
                // Mover sólo en Y para “pegar” el capsule al suelo
                _position.y += deltaY;

                // Si veníamos con velocidad negativa, la cancelamos para que no vuelva a hundirse
                if (_velocity.y < 0f) _velocity.y = 0f;
                return true;
            }

            return false;
        }
        
        /// <summary>Detecta pickups overlapeando el cápsule actual y les notifica.</summary>
        // private void DetectPickups()
        // {
        //     if (_pickupMask == 0) return;
        //     
        //     Vector3 bottom = GetCapsuleBottomAt(_position);
        //     Vector3 top    = GetCapsuleTopAt(_position);
        //     
        //     int count = Physics.OverlapCapsuleNonAlloc(
        //         bottom, top, _capsule.radius,
        //         _pickupHits, _pickupMask,
        //         QueryTriggerInteraction.Collide);
        //
        //     if (count <= 0) return;
        //     
        //     var agent = GetComponentInParent<Player.New.PlayerAgent>();
        //     if (agent == null) return;
        //
        //     for (int i = 0; i < count; i++)
        //     {
        //         var col = _pickupHits[i];
        //         if (col == null) continue;
        //
        //         // Cualquier script de pickup puede implementar este callback
        //         // (evitamos depender de OnTriggerEnter)
        //         col.SendMessage("OnMotorTouch", agent, SendMessageOptions.DontRequireReceiver);
        //     }
        // }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_position,0.1f);
        }
    }
}