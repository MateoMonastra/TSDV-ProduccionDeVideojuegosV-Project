using System;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace CheckPoint
{
    public class KillBound : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            if (!other.TryGetComponent(out ExampleCharacterController characterController)) return;
            characterController.DeathSequence(transform.position);
        }
    }
}
