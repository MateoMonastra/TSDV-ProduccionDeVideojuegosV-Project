using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Dibuja Gizmos de los volúmenes de ataque del player:
    /// - Cono frontal (Attack1/2/3)
    /// - Esfera Spin (SpinRelease)
    /// - Esfera Vertical (AttackVertical)
    /// No modifica gameplay; es solo visualización en Scene.
    /// </summary>
    [ExecuteAlways]
    public class AttackGizmosDrawer : MonoBehaviour
    {
        [Header("Refs")] [SerializeField] private PlayerModel model;
        [SerializeField] private MyKinematicMotor motor;

        [Tooltip("Si no se asigna, se usa el transform de este GameObject.")] [SerializeField]
        private Transform forwardRef;

        [Header("Enable / Disable")] public bool showFrontalCombo = true;
        public bool showSpin = true;
        public bool showVertical = true;
        public bool showVerticalMinHeight = true;

        [Tooltip("Si está activo, solo dibuja cuando el objeto está seleccionado.")]
        public bool onlyWhenSelected = true;

        [Header("Colors")] public Color coneColor = new Color(1f, 0.45f, 0.1f, 0.35f);
        public Color coneEdgeColor = new Color(1f, 0.6f, 0.2f, 0.9f);
        public Color spinColor = new Color(0.1f, 0.7f, 1f, 0.25f);
        public Color spinEdgeColor = new Color(0.2f, 0.9f, 1f, 0.9f);
        public Color verticalColor = new Color(1f, 0.2f, 0.4f, 0.25f);
        public Color verticalEdgeColor = new Color(1f, 0.3f, 0.5f, 0.9f);
        public Color verticalMinOkColor = new Color(0.2f, 1f, 0.2f, 0.9f);
        public Color verticalMinFailColor = new Color(1f, 0.2f, 0.2f, 0.9f);

        [Header("Vertical options")] [Tooltip("Proyecta el centro de la esfera vertical al piso (raycast hacia -up).")]
        public bool verticalProjectToGround = true;

        [Tooltip("Longitud del raycast hacia abajo para encontrar piso.")]
        public float groundProbeLength = 2.0f;

        [Tooltip("Capas consideradas como piso al proyectar el slam.")]
        public LayerMask groundMask = ~0;

        [Header("Cone detail")] [Tooltip("Segmentos para dibujar el arco del cono (más = más suave).")] [Range(6, 64)]
        public int coneSegments = 24;

        private void Reset()
        {
            if (!forwardRef) forwardRef = transform;
            if (!model) model = GetComponent<PlayerModel>();
            if (!motor) motor = GetComponent<MyKinematicMotor>();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying && onlyWhenSelected) return;
            DrawAll();
        }

        private void OnDrawGizmosSelected()
        {
            if (onlyWhenSelected) DrawAll();
        }

        private void DrawAll()
        {
            if (!model) return;
            var t = forwardRef ? forwardRef : transform;
            Vector3 pos = t.position;

            Vector3 up = motor ? motor.CharacterUp : Vector3.up;
            Vector3 fwd = t.forward;
            fwd = Vector3.ProjectOnPlane(fwd, up).normalized;
            if (fwd.sqrMagnitude < 1e-6f) fwd = (Vector3.forward);

            if (showFrontalCombo)
                DrawFrontalCone(pos, fwd, up, model.AttackRange, model.AttackHalfAngleDegrees);

            if (showSpin)
                DrawSphere(pos, model.SpinRadius, spinColor, spinEdgeColor);

            if (showVertical)
            {
                Vector3 center = pos;
                if (verticalProjectToGround)
                {
                    Vector3 down = -up;
                    if (Physics.Raycast(pos, down, out var hit, groundProbeLength, groundMask,
                            QueryTriggerInteraction.Ignore))
                        center = hit.point;
                }

                DrawSphere(center, model.VerticalAttackRadius, verticalColor, verticalEdgeColor);
                
                if (showVerticalMinHeight)
                    DrawVerticalMinHeight(pos, up);
            }
        }

        private void DrawFrontalCone(Vector3 origin, Vector3 forward, Vector3 up, float range, float halfAngleDeg)
        {
            Quaternion qLeft = Quaternion.AngleAxis(-halfAngleDeg, up);
            Quaternion qRight = Quaternion.AngleAxis(halfAngleDeg, up);

            Vector3 leftDir = qLeft * forward;
            Vector3 rightDir = qRight * forward;
            
            Gizmos.color = coneEdgeColor;
            Gizmos.DrawLine(origin, origin + leftDir * range);
            Gizmos.DrawLine(origin, origin + rightDir * range);
            
            Vector3 prev = origin + leftDir * range;
            float step = (halfAngleDeg * 2f) / Mathf.Max(1, coneSegments);
            for (int i = 1; i <= coneSegments; i++)
            {
                Quaternion q = Quaternion.AngleAxis(-halfAngleDeg + i * step, up);
                Vector3 p = origin + (q * forward) * range;
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
            
            Gizmos.color = coneColor;
            int fillRays = Mathf.Clamp(coneSegments / 3, 2, 10);
            float fillStep = (halfAngleDeg * 2f) / (fillRays - 1);
            for (int i = 0; i < fillRays; i++)
            {
                float ang = -halfAngleDeg + i * fillStep;
                Vector3 dir = Quaternion.AngleAxis(ang, up) * forward;
                Gizmos.DrawLine(origin, origin + dir * range);
            }
        }

        private void DrawSphere(Vector3 center, float radius, Color fill, Color edge)
        {
            Gizmos.color = edge;
            
            DrawWireCircle(center, Vector3.up, radius);
            DrawWireCircle(center, Vector3.right, radius);
            DrawWireCircle(center, Vector3.forward, radius);
            
            Gizmos.color = fill;
            int rays = 12;
            for (int i = 0; i < rays; i++)
            {
                float t = (i / (float)rays) * Mathf.PI * 2f;
                Vector3 d = new Vector3(Mathf.Cos(t), 0f, Mathf.Sin(t));
                Gizmos.DrawLine(center, center + d * radius);
            }
        }

        private void DrawWireCircle(Vector3 center, Vector3 normal, float radius)
        {
            const int segs = 32;
            Vector3 axisA = Vector3.Slerp(normal, -normal, 0.5f).normalized;
            if (axisA.sqrMagnitude < 1e-6f) axisA = Vector3.right;
            axisA = Vector3.ProjectOnPlane(axisA, normal).normalized;
            Vector3 axisB = Vector3.Cross(normal, axisA);

            Vector3 prev = center + axisA * radius;
            for (int i = 1; i <= segs; i++)
            {
                float ang = (i / (float)segs) * Mathf.PI * 2f;
                Vector3 p = center + (axisA * Mathf.Cos(ang) + axisB * Mathf.Sin(ang)) * radius;
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
        
        /// <summary>
        /// Dibuja la línea de altura mínima para habilitar el ataque vertical y marca
        /// con color si el suelo está más cerca que el umbral (FAIL) o no (OK).
        /// </summary>
        private void DrawVerticalMinHeight(Vector3 origin, Vector3 up)
        {
            if (!model) return;

            float minDist = model.MinimalGroundDistance;
            if (minDist <= 0f) return;

            Vector3 down = -up;
            float probeLen = Mathf.Max(groundProbeLength, minDist);
            
            bool hitGround = Physics.Raycast(origin, down, out var hit, probeLen, groundMask, QueryTriggerInteraction.Ignore);
            float distToGround = hitGround ? hit.distance : Mathf.Infinity;
            
            Vector3 thresholdPoint = origin + down * minDist;
            Gizmos.color = (distToGround < minDist) ? verticalMinFailColor : verticalMinOkColor;
            Gizmos.DrawLine(origin, thresholdPoint);
            
            float r = 0.15f;
            DrawWireCircle(thresholdPoint, up, r);

            if (hitGround && distToGround > minDist)
            {
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.4f);
                Gizmos.DrawLine(thresholdPoint, hit.point);
                DrawWireCircle(hit.point, up, r * 0.8f);
            }
        }
    }
}
