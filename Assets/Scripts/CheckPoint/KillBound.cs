using System;
using UnityEngine;

namespace CheckPoint
{
    public class KillBound : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("entro");
            if (!other.gameObject.CompareTag("Player")) return;
            Debug.Log("murio");
            GameEvents.PlayerDied(other.gameObject);
        }
    }
}
