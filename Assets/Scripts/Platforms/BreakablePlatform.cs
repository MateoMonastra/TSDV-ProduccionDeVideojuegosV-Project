using System.Collections.Generic;
using PickUps;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Platforms
{
    public class BreakablePlatform : Pickup, IBreakable
    {
        [SerializeField] private Rigidbody[] rbFragments;
        [SerializeField] private UnityEvent OnBreak;
        [SerializeField] private float explosionMinForce;
        [SerializeField] private float explosionMaxForce;
        [SerializeField] private float explosionForceRadius;

        private List<Vector3> rbFragmentsPositions = new();
        private List<Quaternion> rbFragmentsRotations = new();

        private void Start()
        {
            foreach (Rigidbody rb in rbFragments)
            {
                rbFragmentsPositions.Add(rb.transform.localPosition);
                rbFragmentsRotations.Add(rb.transform.localRotation);
            }
        }

        public void Break()
        {
            RefreshCooldown();
            OnBreak?.Invoke();
            ExplodeFragments();
        }

        private void ExplodeFragments()
        {
            foreach (Rigidbody rb in rbFragments)
            {
                rb.AddExplosionForce(Random.Range(explosionMinForce, explosionMaxForce), transform.position + Vector3.up*1.5f,
                    explosionForceRadius);
            }
        }

        public void ResetFragments()
        {
            Debug.Log("Reset Fragments");
            for (int i = 0; i < rbFragments.Length; i++)
            {
                rbFragments[i].transform.localPosition = rbFragmentsPositions[i];
                rbFragments[i].transform.localRotation = rbFragmentsRotations[i];
                
                rbFragments[i].velocity = Vector3.zero;
                rbFragments[i].angularVelocity = Vector3.zero;
            }
        }
    }
}