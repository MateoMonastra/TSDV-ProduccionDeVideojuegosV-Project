using UnityEngine;

namespace Enemies.RangeEnemy
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float damage;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private float lifeTime;

        [Header("Impact VFX")]
        [SerializeField] private GameObject impactParticlesPrefab;
        [SerializeField] private float particlesLifeTime;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayer) != 0)
            {
                //TODO: colocar daño al jugador
                
                SpawnImpactParticles();
                Destroy(gameObject);
            }
            else if (((1 << other.gameObject.layer) & environmentLayer) != 0)
            {
                SpawnImpactParticles();
                Destroy(gameObject);
            }
        }

        private void SpawnImpactParticles()
        {
            if (impactParticlesPrefab != null)
            {
                GameObject particles = Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(particles, particlesLifeTime);
            }
        }
    }
}