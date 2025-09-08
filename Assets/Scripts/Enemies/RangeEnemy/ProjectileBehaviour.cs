using Health;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.RangeEnemy
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Stats")] 
        [SerializeField] private int damage = 1;
        [SerializeField] private (int, int) knockback = (10,15);
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private LayerMask playerLayer;
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
            int otherLayer = other.gameObject.layer;
            
            if (IsInLayerMask(otherLayer, playerLayer))
            {
                if (activateLogs) Debug.Log("Player hit", this);

                var playerHealth = other.GetComponentInParent<HealthController>();
                if (!playerHealth) return;

                playerHealth.Damage(new DamageInfo(damage, transform.position, knockback));

                SpawnImpactParticles();
                Destroy(gameObject);
                return;
            }
            
            if (IsInLayerMask(otherLayer, environmentLayer))
            {
                if (activateLogs) Debug.Log("Environment hit", this);
                SpawnImpactParticles();
                Destroy(gameObject);
                return;
            }
        }
        
        private static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        private void SpawnImpactParticles()
        {
            if (impactParticlesPrefab == null) return;
            GameObject particles = Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(particles, particlesLifeTime);
        }
    }
}