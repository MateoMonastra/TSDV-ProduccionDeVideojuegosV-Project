using System;
using UnityEngine;

namespace CheckPoint
{
    public class KillBound : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameEvents.PlayerDied(other.gameObject);
            }
        }
    }
}
