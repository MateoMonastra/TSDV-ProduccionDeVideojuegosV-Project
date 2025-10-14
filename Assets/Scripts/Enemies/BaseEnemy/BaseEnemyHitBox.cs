using System.Collections.Generic;
using Health;
using UnityEngine;

namespace Enemies.BaseEnemy
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class BaseEnemyHitBox : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private Knockback knockback = new Knockback { horizontal = 20f, vertical = 35f };

        [Header("Filtering")]
        [Tooltip("Solo dañará colliders en estas capas (vacío = todas).")]
        [SerializeField] private LayerMask targetLayers = ~0;

        [Tooltip("Si está activo, sólo aplicará daño a objetos con este Tag.")]
        [SerializeField] private bool requireTag = false;

        [SerializeField] private string requiredTag = "Player";

        [Header("Throttling & Debug")]
        [Tooltip("Evita múltiples golpes al mismo objetivo en una única ventana (segundos).")]
        [Min(0f)] [SerializeField] private float perTargetCooldown = 0.05f;

        [SerializeField] private bool activateLogs = false;

        private Collider _col;
        private Rigidbody _rb;
        
        private readonly Dictionary<HealthController, float> _lastHitTime = new();

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb  = GetComponent<Rigidbody>();
            
            _col.isTrigger  = true;
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }

        private void OnEnable()
        {
            _lastHitTime.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsInLayerMask(other.gameObject.layer, targetLayers))
                return;

            var health = other.GetComponentInParent<HealthController>();
            if (!health) return;

            if (requireTag && !health.gameObject.CompareTag(requiredTag))
                return;
            
            float now = Time.time;
            if (_lastHitTime.TryGetValue(health, out float last) && (now - last) < perTargetCooldown)
                return;

            _lastHitTime[health] = now;

            if (activateLogs)
                Debug.Log($"[EnemyHitBox] Hit {health.name} by {name}", this);
            
            health.Damage(new DamageInfo(
                damage,
                transform.position,
                knockback
            ));
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
