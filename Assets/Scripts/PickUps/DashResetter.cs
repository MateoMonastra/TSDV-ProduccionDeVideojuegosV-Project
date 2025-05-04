using System;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace PickUps
{
    public class DashResetter : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name);
            if (other.tag == "Player")
            {
                Debug.Log("aver");
                if (other.TryGetComponent(out ExampleCharacterController player))
                {
                    player.AddExtraDashCharge();
                    //Destroy(gameObject);
                }
            }
        }
    }
}