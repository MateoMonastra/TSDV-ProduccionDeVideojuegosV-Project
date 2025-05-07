using KinematicCharacterController.Examples;
using UnityEngine;

namespace PickUps
{
    public class JumpRefresher : MonoBehaviour
    {
        [SerializeField] private int extraJumpsToGive = 1;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                if (other.TryGetComponent(out ExampleCharacterController player))
                {
                    player.AddExtraJumps(extraJumpsToGive);
                    Destroy(gameObject);
                }
            }
        }
    }
}