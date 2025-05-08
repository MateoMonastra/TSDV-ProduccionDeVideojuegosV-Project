using System;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace PickUps
{
    public class DashResetter : Pickup
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
           
            if (!other.TryGetComponent(out ExampleCharacterController player)) return;
            
            player.AddExtraDashCharge();
            RefreshCooldown();
        }
    }
}