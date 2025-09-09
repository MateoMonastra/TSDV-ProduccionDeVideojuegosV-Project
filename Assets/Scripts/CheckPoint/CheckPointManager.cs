using Player.New;
using UnityEngine;
using UnityEngine.Serialization;

namespace CheckPoint
{
    public class CheckPointManager : MonoBehaviour
    {
        [SerializeField] private CheckPointManagerRef checkpointManagerRef;
        [SerializeField] protected PlayerModel playerModel;
        
        private bool _hasCheckpoint = false;

        private void OnEnable()
        {
            if (checkpointManagerRef) checkpointManagerRef.manager = this;
        }

        private void OnDisable()
        {
            if (checkpointManagerRef) checkpointManagerRef.manager = null;
        }

        // Set por posición/rotación
        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            playerModel.RespawnPosition = position;
            playerModel.RespawnRotation = rotation;
            _hasCheckpoint = true;
        }
        
        public void SetCheckpoint(Transform t)
        {
            if (!t) return;
            SetCheckpoint(t.position, t.rotation);
        }

        public bool IsLastCheckpoint(Vector3 position)
        {
            return _hasCheckpoint && playerModel.RespawnPosition == position;
        }
    }
}
