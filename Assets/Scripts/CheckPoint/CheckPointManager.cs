using System;
using UnityEngine;

namespace CheckPoint
{
    public class CheckPointManager : MonoBehaviour
    {
        private Vector3 _lastCheckpointPosition;

        [SerializeField] private CheckPointManagerRef checkpointManagerRef;
        private void OnEnable()
        {
            checkpointManagerRef.manager = this;
        }
        
        private void OnDisable()
        {
            checkpointManagerRef.manager = null;
        }

        public void SetCheckpoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            Debug.Log("New CheckPoint: " + position);
        }

        public void Respawn(GameObject player)
        {
            if (_lastCheckpointPosition != Vector3.zero)
            {
                player.transform.position = _lastCheckpointPosition;
            }
            else
            {
                Debug.LogWarning("No se ha establecido ningún checkpoint aún.");
            }
        }
    }
}
