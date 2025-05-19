using KinematicCharacterController.Examples;
using UnityEngine;

namespace Enemies.RangeEnemy
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float damage;
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private float lifeTime;

        [Header("Impact VFX")]
        [SerializeField] private GameObject impactParticlesPrefab;
        [SerializeField] private float particlesLifeTime;

        [Header("Debug")]
        [SerializeField] private bool activateLogs;
        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //TODO: colocar daño al jugador
                if(activateLogs)
                    Debug.Log("Player hit");
                
                if (!other.TryGetComponent(out ExampleCharacterController characterController)) return;
                characterController.DeathSequence(transform.position);
                GameEvents.PlayerDied(other.gameObject);
                SpawnImpactParticles();
                Destroy(gameObject);
            }
            else if (environmentLayer != 0)
            {
                if(activateLogs)
                    Debug.Log("Environment hit");
                
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