using System;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace PickUps
{
    public class DashResetter : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                if (other.TryGetComponent(out ExampleCharacterController player))
                {
                    player.AddExtraDashCharge();
                    Destroy(gameObject);
                }
            }
        }
    }
}