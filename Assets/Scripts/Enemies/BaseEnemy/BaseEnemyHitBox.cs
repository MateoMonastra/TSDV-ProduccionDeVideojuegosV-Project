using Health;
using UnityEngine;

namespace Enemies.BaseEnemy
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class BaseEnemyHitBox : MonoBehaviour
    {
        [System.Serializable]
        public struct Knockback
        {
            public int horizontal; 
            public int vertical;
        }

        [Header("Stats")]
        [SerializeField] private int damage = 1;
        [SerializeField] private Knockback knockback = new Knockback { horizontal = 20, vertical = 35 };

        [Header("Debug")]
        [SerializeField] private bool activateLogs = false;
        [SerializeField] private bool requirePlayerTag = false;

        private Collider _col;
        private Rigidbody _rb;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb  = GetComponent<Rigidbody>();

    
            _col.isTrigger   = true;
            _rb.isKinematic  = true;
            _rb.useGravity   = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var health = other.GetComponentInParent<HealthController>();
            if (!health) return;
            
            if (requirePlayerTag)
            {
                var root = health.gameObject;
                if (!root.CompareTag("Player")) return;
            }

            if (activateLogs)
                Debug.Log($"[EnemyHitBox] Player hit by {name}", this);
            
            health.Damage(new DamageInfo(damage, transform.position, (knockback.horizontal, knockback.vertical)));
        }
    }
}
