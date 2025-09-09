using Player.New;
using UnityEngine;

// PlayerAgent

namespace Platforms
{
    /// <summary>
    /// Plataforma kinemática compatible con tu KCC:
    /// - Se mueve (traslación/rotación/oscilación) en FixedUpdate.
    /// - “Arrastra” al jugador que está ENCIMA usando el delta de la plataforma,
    ///   llamando a MyKinematicMotor.ApplyPlatformMotion (no pisa la velocidad).
    /// - Detección de riders por OverlapBox + confirmación con raycast corto hacia esta plataforma.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField, Tooltip("Eje local (o mundo) en el que ping-ponguea la traslación.")]
        private Vector3 translationAxis = Vector3.right;

        [SerializeField, Tooltip("Distancia total de ping-pong.")]
        private float translationDistance = 10f;

        [SerializeField, Tooltip("Velocidad de ping-pong (unidades/seg).")]
        private float translationSpeed = 1f;

        [Header("Rotación")]
        [SerializeField, Tooltip("Eje de rotación (en espacio del objeto).")]
        private Vector3 rotationAxis = Vector3.up;

        [SerializeField, Tooltip("Velocidad de rotación (grados/seg).")]
        private float rotationSpeed = 10f;

        [Header("Oscilación (opcional)")]
        [SerializeField, Tooltip("Eje de oscilación angular (en espacio del objeto).")]
        private Vector3 oscillationAxis = Vector3.zero;

        [SerializeField, Tooltip("Amplitud de oscilación angular (grados).")]
        private float oscillationAmplitude = 0f;

        [SerializeField, Tooltip("Frecuencia de oscilación angular (Hz aprox.).")]
        private float oscillationSpeed = 0f;

        [Header("Arrastre del jugador")]
        [SerializeField, Tooltip("Distancia del raycast hacia abajo para confirmar que el player está apoyado sobre ESTA plataforma.")]
        private float groundedCheckDistance = 0.25f;

        [SerializeField, Tooltip("Capas consideradas como ‘suelo’ para el raycast de confirmación.")]
        private LayerMask groundMask = ~0;

        [SerializeField, Tooltip("Capa(s) del Player usadas en el OverlapBox.")]
        private LayerMask playerLayer = ~0;

        [SerializeField, Tooltip("Altura del volumen de detección por encima del collider de la plataforma.")]
        private float riderProbeHeight = 0.25f;

        [SerializeField, Tooltip("Si es true, aplica también el delta de rotación de la plataforma al rider (y rota su velocidad).")]
        private bool carryRotation = true;

        [Header("Debug")]
        [SerializeField, Tooltip("Imprimir logs en consola cuando arrastra al rider.")]
        private bool logs = false;
    
        private Rigidbody _rb;
        private Collider _col;

        private Vector3 _basePos;
        private Quaternion _baseRot;

        private Vector3 _prevPos;
        private Quaternion _prevRot;
    
        private void Awake()
        {
            CacheComponents();
            ConfigureAsKinematicSolid();
            CacheBasePose();
            ResetPrevPose();
        }

        private void FixedUpdate()
        {
            // 1) Calcular pose objetivo
            float t = Time.time;
            var goalRot = ComputeGoalRotation(t);
            var goalPos = ComputeGoalPosition(t);

            // 2) Mover plataforma
            MovePlatform(goalPos, goalRot);

            // 3) Delta de plataforma
            Vector3 deltaPos; Quaternion deltaRot;
            ComputeDeltas(goalPos, goalRot, out deltaPos, out deltaRot);

            // 4) Arrastrar riders (si hubo delta apreciable)
            TryMoveRiders(deltaPos, deltaRot);

            // 5) Preparar el siguiente frame
            StorePrevPose(goalPos, goalRot);
        }
    
        /// <summary>Cachea referencias de Collider y Rigidbody.</summary>
        private void CacheComponents()
        {
            _col = GetComponent<Collider>();
            _rb  = GetComponent<Rigidbody>();
        }

        /// <summary>Configura la plataforma como colisionable sólida y cinemática.</summary>
        private void ConfigureAsKinematicSolid()
        {
            _col.isTrigger  = false;
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }

        /// <summary>Guarda la pose base (inicio) para componer el movimiento.</summary>
        private void CacheBasePose()
        {
            _basePos = _rb.position;
            _baseRot = _rb.rotation;
        }

        /// <summary>Inicializa la pose previa con la base.</summary>
        private void ResetPrevPose()
        {
            _prevPos = _basePos;
            _prevRot = _baseRot;
        }
    
        /// <summary>Calcula la posición objetivo usando ping-pong en el eje indicado.</summary>
        private Vector3 ComputeGoalPosition(float t)
        {
            float dist = Mathf.Max(0f, translationDistance);
            float moveAmount = Mathf.PingPong(t * translationSpeed, dist);
            return _basePos + translationAxis.normalized * moveAmount;
        }

        /// <summary>Calcula la rotación objetivo combinando rotación constante + oscilación angular opcional.</summary>
        private Quaternion ComputeGoalRotation(float t)
        {
            var axisRot = rotationAxis.sqrMagnitude > 1e-6f ? rotationAxis.normalized : Vector3.up;
            Quaternion rotConstant = Quaternion.AngleAxis(rotationSpeed * t, axisRot);

            Quaternion rotOsc = Quaternion.identity;
            if (oscillationAmplitude > 0f && oscillationSpeed > 0f && oscillationAxis.sqrMagnitude > 1e-6f)
            {
                float ang = Mathf.Sin(t * oscillationSpeed) * oscillationAmplitude;
                rotOsc = Quaternion.AngleAxis(ang, oscillationAxis.normalized);
            }

            return rotConstant * _baseRot * rotOsc;
        }
    
        /// <summary>Mueve la plataforma kinemática a la pose indicada.</summary>
        private void MovePlatform(in Vector3 goalPos, in Quaternion goalRot)
        {
            _rb.MovePosition(goalPos);
            _rb.MoveRotation(goalRot);
        }

        /// <summary>Calcula los deltas de posición y rotación respecto del frame anterior.</summary>
        private void ComputeDeltas(in Vector3 goalPos, in Quaternion goalRot, out Vector3 deltaPos, out Quaternion deltaRot)
        {
            deltaPos = goalPos - _prevPos;
            deltaRot = goalRot * Quaternion.Inverse(_prevRot);
        }

        /// <summary>Persiste la pose actual para el cálculo de deltas del próximo frame.</summary>
        private void StorePrevPose(in Vector3 pos, in Quaternion rot)
        {
            _prevPos = pos;
            _prevRot = rot;
        }
    
        /// <summary>
        /// Si hubo delta de plataforma, detecta riders por OverlapBox, confirma apoyo con raycast
        /// y aplica el delta usando MyKinematicMotor.ApplyPlatformMotion.
        /// </summary>
        private void TryMoveRiders(in Vector3 deltaPos, in Quaternion deltaRot)
        {
            if (!HasAppreciableDelta(deltaPos, deltaRot))
                return;

            // Volumen de detección encima del collider
            var b = _col.bounds; // mundo
            Vector3 center = new Vector3(b.center.x, b.max.y + riderProbeHeight * 0.5f, b.center.z);
            Vector3 halfExtents = new Vector3(b.extents.x, riderProbeHeight * 0.5f, b.extents.z);

            var hits = Physics.OverlapBox(center, halfExtents, transform.rotation, playerLayer, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0) return;

            Vector3 up = transform.up;
            for (int i = 0; i < hits.Length; i++)
            {
                var agent = hits[i].GetComponentInParent<PlayerAgent>();
                if (!agent) continue;

                var motor = agent.GetComponent<MyKinematicMotor>();
                if (!motor) continue;

                if (!IsStandingOnThisPlatform(agent, up))
                    continue;

                ApplyDeltaToRider(motor, deltaPos, deltaRot);

                if (logs)
                    Debug.Log($"[Platform-Overlap] Move rider {agent.name} Δpos={deltaPos}, rot={(carryRotation ? "on" : "off")}", this);
            }
        }

        /// <summary>Devuelve true si hubo movimiento o rotación “suficiente” para justificar arrastrar riders.</summary>
        private static bool HasAppreciableDelta(in Vector3 deltaPos, in Quaternion deltaRot)
        {
            return deltaPos.sqrMagnitude > 1e-8f || Quaternion.Angle(Quaternion.identity, deltaRot) > 0.01f;
        }

        /// <summary>
        /// Confirma que el agente sigue apoyado SOBRE esta plataforma
        /// con un raycast corto en la “up” de la plataforma.
        /// </summary>
        private bool IsStandingOnThisPlatform(PlayerAgent agent, in Vector3 platformUp)
        {
            Vector3 origin = agent.transform.position + platformUp * 0.05f;
            if (!Physics.Raycast(origin, -platformUp, out RaycastHit rh, groundedCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
                return false;

            return rh.collider == _col;
        }

        /// <summary>
        /// Aplica el delta de la plataforma al motor del jugador,
        /// sin resetear velocidad (y rotándola si carryRotation = true).
        /// </summary>
        private void ApplyDeltaToRider(MyKinematicMotor motor, in Vector3 deltaPos, in Quaternion deltaRot)
        {
            Quaternion riderDeltaRot = carryRotation ? deltaRot : Quaternion.identity;
            motor.ApplyPlatformMotion(deltaPos, riderDeltaRot, rotateVelocity: carryRotation);
        }
    }
}
