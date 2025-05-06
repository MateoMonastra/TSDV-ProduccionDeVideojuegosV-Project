using System;
using KinematicCharacterController.Examples;
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
                ExampleCharacterController characterController = player.GetComponent<ExampleCharacterController>();

                if (characterController)
                {
                    characterController.Motor.SetPositionAndRotation(_lastCheckpointPosition, Quaternion.identity);
                }
            }
            else
            {
                Debug.LogWarning("No se ha establecido ningún checkpoint aún.");
            }
        }

        public bool IsLastCheckpoint(Vector3 position)
        {
            if (_lastCheckpointPosition == position)
            {
                Debug.Log("the same");
            }
            else
            {
                Debug.Log("not the same");
            }

            return _lastCheckpointPosition == position;
        }
    }
}