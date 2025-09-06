using UnityEngine;
using Health;

namespace CheckPoint
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class KillBound : MonoBehaviour
    {
        private Collider _col;
        private Rigidbody _rb;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;              

            _rb = GetComponent<Rigidbody>();    
            _rb.isKinematic = true;           
            _rb.useGravity  = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var health = other.GetComponentInParent<HealthController>();
            if (health == null)
            {
                Debug.LogWarning($"KillBound: no se encontró HealthController en la jerarquía de {other.name}", other);
                return;
            }

            health.InstaKill(); // Esto debe disparar OnDeath en HealthController
        }
    }
}