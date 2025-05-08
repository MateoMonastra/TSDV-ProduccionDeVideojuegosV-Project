using CheckPoint;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelManager
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private CheckPointManager checkPointManager;
        
        private void OnEnable()
        {
            GameEvents.OnPlayerDied += checkPointManager.Respawn;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= checkPointManager.Respawn;
        }
        
    }
}