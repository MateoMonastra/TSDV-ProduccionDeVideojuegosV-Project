using System;
using UnityEngine;

namespace CheckPoint
{
    public class KillBound : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameEvents.PlayerDied(other.gameObject);
            }
        }
    }
}
