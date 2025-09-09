using UnityEngine;
using Player.New;

namespace PickUps
{
    public class DashRefresher : Pickup
    {
        // [Header("Buff (valores absolutos)")]
        // [SerializeField, Tooltip("Distancia del PRÓXIMO dash (m).")]
        // private float buffDistance = 3.0f;
        //
        // [SerializeField, Tooltip("Velocidad del PRÓXIMO dash (m/s).")]
        // private float buffSpeed = 24f;

        [Header("FX / UI (opcionales)")]
        [SerializeField] private ParticleSystem pickUpParticles;
        [SerializeField] private AudioSource sfx;

        private void OnTriggerEnter(Collider other)
        {
            var agent = other.GetComponentInParent<PlayerAgent>();
            if (agent != null) OnMotorTouch(agent);
        }

        // Si usás el callback del motor kinemático:
        public void OnMotorTouch(PlayerAgent agent)
        {
            if (agent == null) return;
            var model = agent.GetPlayerModel();

            // Si ya hay un buff pendiente, ignorar hasta que se consuma
            if (model.DashBuffPending) return;

            // Setear valores absolutos para el PRÓXIMO dash
            model.DashBuffPending  = true;
            //model.DashBuffDistance = buffDistance;
            //model.DashBuffSpeed    = buffSpeed;

            // Resetear cooldown: usable YA
            model.DashCooldownLeft = 0f;
            model.DashOnCooldown   = false;

            // FX/UI
            if (pickUpParticles)
            {
                if (pickUpParticles.isPlaying) pickUpParticles.Stop();
                pickUpParticles.Play();
            }
            if (sfx) sfx.Play();
            
             RefreshCooldown();
        }
    }
}
