using System;
using Health;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace CheckPoint
{
    public class KillBound : MonoBehaviour
    {
        [SerializeField] private (int, int) knockback = (20,35);
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            if (!other.TryGetComponent(out HealthController controller)) return;
            controller.InstaKill(new DamageInfo(0, transform.position,knockback));
        }
    }
}