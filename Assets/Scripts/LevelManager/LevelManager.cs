using CheckPoint;
using UnityEngine;

namespace LevelManager
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private CheckPointManager checkPointManager;

        private void Awake()
        {
            GameEvents.GameEvents.LevelStarted();
        }
    }
}