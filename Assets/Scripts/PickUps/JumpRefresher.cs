using KinematicCharacterController.Examples;
using UnityEngine;

namespace PickUps
{
    public class JumpRefresher : Pickup
    {
        [SerializeField] private ParticleSystem pickUpParticles;
        [SerializeField] private int extraJumpsToGive = 1;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (!other.TryGetComponent(out ExampleCharacterController player)) return;

            player.AddExtraJumps(extraJumpsToGive);
            RefreshCooldown();
            
            if(pickUpParticles.isPlaying)
                pickUpParticles.Stop();
            
            pickUpParticles.Play();
        }
    }
}