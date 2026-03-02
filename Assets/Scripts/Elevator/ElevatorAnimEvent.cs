using UnityEngine;

namespace Elevator
{
    public class ElevatorAnimEvent : MonoBehaviour
    {
        public void OnEndAnimation()
        {
            var cinematic = GetComponent<ElevatorCinematic>();
            if (cinematic != null)
            {
                cinematic.EndCinematic();
            }
        }
    }
}