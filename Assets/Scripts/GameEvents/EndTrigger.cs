using System;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEvents.GameEvents.LevelEnded();
            Debug.Log("SASA");
        }
    }
}
