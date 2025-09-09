using UnityEngine;
using Player.New;

namespace PickUps
{
    /// <summary>
    /// Otorga 1 salto extra. Se consume como 3er salto cuando JumpsLeft == 0.
    /// Si el jugador ya tiene un salto extra pendiente, ignora el pickup.
    /// </summary>
    public class JumpRefresher : Pickup
    {
        [Header("FX / UI (opcionales)")]
        [SerializeField] private ParticleSystem pickUpParticles;
        [SerializeField] private AudioSource sfx;

        /// <summary>
        /// Llamado por el motor kinemático mediante SendMessage cuando
        /// el capsule del jugador solapa este pickup.
        /// </summary>
        public void OnMotorTouch(PlayerAgent agent)
        {
            if (agent == null) return;
            var model = agent.GetPlayerModel();
            
            if (model.HasExtraJump) return;
            
            model.HasExtraJump = true;
            
            if (pickUpParticles)
            {
                if (pickUpParticles.isPlaying) pickUpParticles.Stop();
                pickUpParticles.Play();
            }
            if (sfx) sfx.Play();

            RefreshCooldown();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            var agent = other.GetComponentInParent<PlayerAgent>();
            if (agent != null) OnMotorTouch(agent);
        }
    }
}