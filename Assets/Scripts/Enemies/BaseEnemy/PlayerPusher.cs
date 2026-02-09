using System;
using Player.New;
using UnityEngine;

public class PlayerPusher : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if(other.TryGetComponent(out MyKinematicMotor playerMotor))
        {
            playerMotor.AddVelocity(transform.forward * 5.0f);
        }
    }
}
