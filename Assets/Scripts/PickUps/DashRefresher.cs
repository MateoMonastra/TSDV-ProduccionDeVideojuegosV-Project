using UnityEngine;
using Player.New;

namespace PickUps
{
    /// <summary>
    /// Marca un buff para el próximo dash (multiplica la distancia) y
    /// resetea el cooldown a 0 para permitir usarlo ya.
    /// Si el jugador ya tiene un dash buff pendiente, ignora el pickup.
    /// </summary>
    public class DashRefresher : Pickup
    {
        [Header("Buff")]
        [SerializeField, Tooltip("Override opcional del multiplicador; si <= 0 usa el del PlayerModel.")]
        private float distanceMultiplierOverride = 0f;

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
            
            if (model.DashBuffPending) return;

            model.DashBuffPending = true;
            
            if (distanceMultiplierOverride > 0f)
                model.DashBuffDistanceMultiplier = distanceMultiplierOverride;
            
            model.DashCooldownLeft = 0f;
            model.DashOnCooldown   = false;
            
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