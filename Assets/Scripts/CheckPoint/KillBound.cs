using UnityEngine;
using Health;

namespace CheckPoint
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class KillBound : MonoBehaviour
    {
        [SerializeField] private string name = "Water";
        private Rigidbody _rb;
        private Collider _col;

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
            if (!health) return;

            health.InstaKill(name);
        }
    }
}