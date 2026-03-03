using System;
using System.Collections.Generic;
using Health;
using UnityEngine;
using UnityEngine.Events;

namespace Hazards.FlameThrower
{
    public class FireBreatherAgent : MonoBehaviour
    {
        public UnityEvent onStartFireEvent;
        public UnityEvent onStopFireEvent;
        public UnityEvent fireTickEvent;
        public UnityEvent chargingTickEvent;

        [SerializeField] private float delayTime;
        [SerializeField] private float gasTime;
        [SerializeField] private float fireTime;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 extents;
        [SerializeField] private Vector2 knockbackForce;

        private float _elapsed;

        private bool delaying = true;
        private bool breathingFire = false;

        private HashSet<HealthController> fireTargets = new HashSet<HealthController>();

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerRevived += ClearList;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerRevived -= ClearList;
        }

        public void ClearList()
        {
            fireTargets.Clear();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            if (delaying)
            {
                if (_elapsed >= delayTime)
                {
                    delaying = false;
                    _elapsed = 0;
                }
                else
                {
                    return;
                }
            }


            if (breathingFire)
            {
                BreatheFire();
                if (_elapsed >= fireTime)
                {
                    _elapsed = 0;
                    breathingFire = !breathingFire;
                    onStopFireEvent?.Invoke();
                    fireTargets?.Clear();
                }
            }
            else
            {
                ChargeFire();
                if (_elapsed >= fireTime)
                {
                    _elapsed = 0;
                    breathingFire = !breathingFire;
                    onStartFireEvent?.Invoke();
                    fireTargets?.Clear();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + center, extents / 2.0f);
            Gizmos.color = Color.green;
        }

        private void BreatheFire()
        {
            fireTickEvent?.Invoke();

            Collider[] colliders = Physics.OverlapBox(transform.position + center, extents / 2.0f, Quaternion.identity,
                LayerMask.GetMask("Player"));

            foreach (Collider collider in colliders)
            {
                //var health = collider.GetComponentInParent<HealthController>();
                var health = collider.GetComponent<HealthController>();
                if (!health) return;

                if (fireTargets.Contains(health))
                    return;
                else
                    fireTargets.Add(health);

                var root = health.gameObject;
                if (!root.CompareTag("Player")) return;

                health.InstaKill("FireBreath");
            }
        }

        private void ChargeFire()
        {
            chargingTickEvent?.Invoke();
        }
    }
}