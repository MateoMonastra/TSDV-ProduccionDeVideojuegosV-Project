using UnityEngine;

namespace CheckPoint
{
    [CreateAssetMenu(fileName = "CheckpointManagerRef", menuName = "Refs/CheckpointManagerRef")]
    public class CheckPointManagerRef : ScriptableObject
    {
        public CheckPointManager manager;
    }
}
