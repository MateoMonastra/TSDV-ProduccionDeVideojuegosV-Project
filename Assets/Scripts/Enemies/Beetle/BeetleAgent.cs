using System.Collections;
using Enemies.BaseEnemy;
using Health;
using UnityEngine;

namespace Enemies.Beetle
{
    public class BeetleAgent : MonoBehaviour
    {
        [Header("Orbit Center")]
        [SerializeField] private Transform center;

        [Header("Orbit Settings")]
        [SerializeField] private float radius = 3f;
        [SerializeField] private float speedDeg = 120f;
        [SerializeField] private bool clockwise = true;

        [Header("Stats")]
        [SerializeField] private int damage = 1;
        [SerializeField] private BaseEnemyHitBox.Knockback knockback = new BaseEnemyHitBox.Knockback { horizontal = 20, vertical = 35 };

        [Header("Vfx")]
        [SerializeField] private GameObject deathVfx;

        [Header("Death Shrink")]
        [SerializeField] private float shrinkDuration = 4f;
        [SerializeField] private AnimationCurve shrinkCurve = null;

        private float _angleRad;
        private float _fixedY;

        private readonly bool _faceMoveDirection = true;
        private readonly float _turnSpeedDeg = 999f;

        private bool _dying;
        private Vector3 _initialScale;
        private Collider _col;
        private BeetleAnimationController _animationController;

        private void Awake()
        {
            if (center == null) center = transform;

            _fixedY = transform.position.y;
            _initialScale = transform.localScale;
            _col = GetComponent<Collider>();
            _animationController = GetComponent<BeetleAnimationController>();

            Vector3 toMe = transform.position - center.position;
            Vector2 xz = new Vector2(toMe.x, toMe.z);

            if (xz.sqrMagnitude < 0.0001f)
                xz = Vector2.right * radius;

            _angleRad = Mathf.Atan2(xz.y, xz.x);
        }

        private void Update()
        {
            if (_dying) return;

            float dir = clockwise ? -1f : 1f;

            _angleRad += dir * speedDeg * Mathf.Deg2Rad * Time.deltaTime;

            float x = Mathf.Cos(_angleRad) * radius;
            float z = Mathf.Sin(_angleRad) * radius;

            Vector3 pos = center.position + new Vector3(x, 0f, z);
            pos.y = _fixedY;
            transform.position = pos;

            if (_faceMoveDirection)
            {
                Vector3 tangent = new Vector3(-Mathf.Sin(_angleRad), 0f, Mathf.Cos(_angleRad)) * dir;

                if (tangent.sqrMagnitude > 0.00001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(tangent.normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeedDeg * Time.deltaTime);
                }
            }
            
        }

        public void OnBeingAttacked()
        {
            if (_dying) return;
            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            _dying = true;
            _animationController.PlayDead();
            
            if (_col) _col.enabled = false;
            
            float t = 0f;
            while (t < shrinkDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / shrinkDuration);
                
                float k = (shrinkCurve != null) ? shrinkCurve.Evaluate(a) : a;
                
                transform.localScale = Vector3.Lerp(_initialScale, Vector3.zero, k);

                yield return null;
            }

            transform.localScale = Vector3.zero;

            if (deathVfx != null)
                Instantiate(deathVfx, transform.position,deathVfx.transform.rotation);
            
            
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_initialScale != Vector3.zero)
                transform.localScale = _initialScale;

            _dying = false;

            if (_col) _col.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_dying) return;

            var health = other.GetComponentInParent<HealthController>();
            if (!health) return;

            var root = health.gameObject;
            if (!root.CompareTag("Player")) return;

            _animationController.PlayAttack();
            health.Damage(new DamageInfo(
                damage,
                transform.position,
                new Vector2(knockback.horizontal, knockback.vertical),
                "BeetleAttack"
            ));
            
        }
    }
}