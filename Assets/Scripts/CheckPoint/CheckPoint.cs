using System;
using UnityEngine;

namespace CheckPoint
{
    public class Checkpoint : MonoBehaviour
    {
        private bool _isActivated = false;
        [SerializeField] private CheckPointManagerRef checkpointManagerRef;

        private void ActivateCheckpoint(Transform player)
        {
            if (_isActivated) return;
            _isActivated = true;

            Vector3 safePosition = CalculateSafeRespawnPosition(player);
            
            if (!checkpointManagerRef.manager.IsLastCheckpoint(safePosition))
            {
                checkpointManagerRef.manager.SetCheckpoint(safePosition);

                Debug.Log("Checkpoint activado en: " + safePosition);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            ActivateCheckpoint(other.transform);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            ActivateCheckpoint(other.transform);
        }

        private Vector3 CalculateSafeRespawnPosition(Transform player)
        {
            Collider checkpointCol = GetComponent<Collider>();
            Collider playerCol = player.GetComponent<Collider>();

            Vector3 checkpointCenter = checkpointCol.bounds.center;
            float checkpointTop = checkpointCol.bounds.max.y;

            float playerHeight = playerCol.bounds.size.y;
            Vector3 finalPosition = new Vector3(
                checkpointCenter.x,
                checkpointTop + (playerHeight / 2f),
                checkpointCenter.z
            );

            return finalPosition;
        }
    }
}
