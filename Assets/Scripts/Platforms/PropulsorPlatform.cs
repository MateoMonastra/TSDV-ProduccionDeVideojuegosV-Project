using KinematicCharacterController;
using UnityEngine;

namespace Platforms
{
    public class PropulsorPlatform : MonoBehaviour
    {
        public float jumpForce;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var kinematicCharacterMotor = other.gameObject.GetComponent<KinematicCharacterMotor>();
                    
                
                if (kinematicCharacterMotor != null)
                {
                    kinematicCharacterMotor.ForceUnground();
                    kinematicCharacterMotor.BaseVelocity = transform.up * jumpForce;
                }
            }
        }
    }
}