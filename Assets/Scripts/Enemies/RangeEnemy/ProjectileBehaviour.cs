using Health;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Enemies.RangeEnemy
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Stats")] 
        [SerializeField] private int damage = 1;
        [SerializeField] private (int, int) knockback = (10,15);
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private float lifeTime;

        [Header("Impact VFX")] [SerializeField]
        private GameObject impactParticlesPrefab;

        [SerializeField] private float particlesLifeTime;

        [Header("Debug")] [SerializeField] private bool activateLogs;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (activateLogs)
                    Debug.Log("Player hit");

                if (!other.TryGetComponent(out HealthController controller)) return;
                controller.Damage(new DamageInfo(damage, -transform.position, knockback));
                SpawnImpactParticles();
                Destroy(gameObject);
            }
            else if (environmentLayer != 0)
            {
                if (activateLogs)
                    Debug.Log("Environment hit");

                SpawnImpactParticles();
                Destroy(gameObject);
            }
        }

        private void SpawnImpactParticles()
        {
            if (impactParticlesPrefab == null) return;
            GameObject particles = Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(particles, particlesLifeTime);
        }
    }
}