using System.Collections;
using Health;
using Player.New;
using UnityEngine;

namespace Hazards.ConfettiMine
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ConfettiMine : MonoBehaviour
    {
        [System.Serializable]
        public struct Knockback { public float horizontal; public float vertical; }

        [Header("Armado")]
        [SerializeField] private float activationDelay = 2f;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private Material warningMaterial;
        [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.Linear(0, 0.2f, 1, 0.05f);
        [SerializeField] private float minBlinkInterval = 0.03f;

        [Header("Explosión")]
        [SerializeField] private float mineRadius = 4f;
        [SerializeField] private Knockback knockback = new Knockback { horizontal = 20f, vertical = 8f };
        [SerializeField] private LayerMask affectedMask = ~0;
        [SerializeField] private bool pushRigidbodies = true;
        [SerializeField] private float rigidbodyImpulse = 12f;
        [SerializeField] private float rigidbodyUpFactor = 0.5f;

        [Header("Detección")]
        [SerializeField, Tooltip("Capas que arman la mina (collider del player o su hijo).")]
        private LayerMask armingMask;
        [SerializeField, Tooltip("Si la máscara no matchea, usar tag 'Player' como fallback.")]
        private bool allowPlayerTagFallback = true;

        [Header("Refs")]
        [SerializeField] private Renderer mineRenderer;
        [SerializeField] private Collider triggerCol;
        [SerializeField] private ParticleSystem[] explosionEffects;

        [Header("Debug")]
        [SerializeField] private bool activateLogs = false;
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoArmed = new Color(1f, 0.2f, 0.2f, 0.6f);
        [SerializeField] private Color gizmoIdle  = new Color(1f, 1f, 1f, 0.25f);

        private bool _isTriggered;
        private Coroutine _warningCoroutine;
        private Coroutine _explodeCoroutine;
        private Rigidbody _rb;

        private void OnValidate()
        {
            if (!triggerCol) triggerCol = GetComponent<Collider>();
            if (!mineRenderer) mineRenderer = GetComponentInChildren<Renderer>(true);
            
            if (armingMask.value == 0)
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer >= 0) armingMask = (1 << playerLayer);
            }
        }

        private void Awake()
        {
            if (!triggerCol) triggerCol = GetComponent<Collider>();
            triggerCol.isTrigger = true;

            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;

            GameEvents.GameEvents.OnPlayerDied += Respawn;
        }

        private void OnDestroy()
        {
            GameEvents.GameEvents.OnPlayerDied -= Respawn;
        }

        private void OnEnable()
        {
            if (mineRenderer && baseMaterial) mineRenderer.material = baseMaterial;
            if (triggerCol) triggerCol.enabled = true;
            if (mineRenderer) mineRenderer.enabled = true;
            _isTriggered = false;
        }

        private static bool InMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

        private void OnTriggerEnter(Collider other) => TryArm(other, "Enter");
        private void OnTriggerStay(Collider other)  => TryArm(other, "Stay");

        private void TryArm(Collider other, string phase)
        {
            if (_isTriggered) return;
            
            bool layerOk = InMask(other.gameObject.layer, armingMask);
            
            bool tagOk = allowPlayerTagFallback 
                         && other.CompareTag("Player") 
                         && other.GetComponentInParent<HealthController>();

            if (!layerOk && !tagOk)
            {
                if (activateLogs)
                {
                    Debug.Log(
                        $"[ConfettiMine] NO ARMADA ({phase}) por {other.name} | " +
                        $"layer={LayerMask.LayerToName(other.gameObject.layer)} fuera de armingMask " +
                        $"{(allowPlayerTagFallback ? "y tag!=Player/HealthController ausente" : "(sin fallback por tag)")}", this);
                }
                return;
            }

            _isTriggered = true;
            if (activateLogs) Debug.Log($"[ConfettiMine] ARMED by {other.name} ({phase})", this);

            if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
            _warningCoroutine = StartCoroutine(ActivateMine());
        }



        private IEnumerator ActivateMine()
        {
            float elapsed = 0f;
            bool toggle = false;

            while (elapsed < activationDelay)
            {
                if (mineRenderer)
                    mineRenderer.material = toggle ? baseMaterial : warningMaterial;
                toggle = !toggle;

                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, activationDelay));
                float wait = Mathf.Max(minBlinkInterval, blinkCurve.Evaluate(normalized));
                yield return new WaitForSeconds(wait);
                elapsed += wait;
            }

            _explodeCoroutine = StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            if (explosionEffects != null)
                foreach (var fx in explosionEffects) if (fx) fx.Play();

            if (triggerCol) triggerCol.enabled = false;
            if (mineRenderer) mineRenderer.enabled = false;

            Vector3 center = transform.position;
            var hits = Physics.OverlapSphere(center, mineRadius, affectedMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                
                var motor = h.GetComponentInParent<MyKinematicMotor>();
                if (motor)
                {
                    Vector3 to = h.bounds.center - center;
                    Vector3 up = motor.CharacterUp;
                    Vector3 horiz = Vector3.ProjectOnPlane(to, up);
                    if (horiz.sqrMagnitude > 1e-6f) horiz.Normalize();
                    Vector3 vel = horiz * knockback.horizontal + up * knockback.vertical;
                    motor.ForceUnground(1);
                    motor.SetVelocity(vel);
                }
                
                if (pushRigidbodies)
                {
                    var rb = h.attachedRigidbody ?? h.GetComponentInParent<Rigidbody>();
                    if (rb && !rb.isKinematic)
                    {
                        Vector3 dir = (h.bounds.center - center).normalized;
                        Vector3 pushDir = (dir + Vector3.up * rigidbodyUpFactor).normalized;
                        rb.AddForce(pushDir * rigidbodyImpulse, ForceMode.VelocityChange);
                    }
                }
            }

            yield break;
        }

        private void Respawn(GameObject _)
        {
            if (_explodeCoroutine != null) StopCoroutine(_explodeCoroutine);
            if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);

            if (mineRenderer)
            {
                mineRenderer.enabled = true;
                if (baseMaterial) mineRenderer.material = baseMaterial;
            }
            if (triggerCol) triggerCol.enabled = true;

            _isTriggered = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            Gizmos.color = _isTriggered ? gizmoArmed : gizmoIdle;
            Gizmos.DrawWireSphere(transform.position, mineRadius);
        }
    }
}
