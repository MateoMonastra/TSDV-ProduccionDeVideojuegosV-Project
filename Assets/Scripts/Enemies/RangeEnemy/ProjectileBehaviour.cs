using Health;
using UnityEngine;

namespace Enemies.RangeEnemy
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        [System.Serializable]
        public struct Knockback
        {
            public int horizontal; // componente horizontal del empuje (m/s aprox)
            public int vertical;   // componente vertical del empuje (m/s aprox)
        }

        [Header("Stats")]
        [SerializeField] private int damage = 1;
        [SerializeField] private Knockback knockback = new Knockback { horizontal = 10, vertical = 15 };
        [SerializeField] private LayerMask environmentMask;
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private float lifeTime = 6f;

        [Header("Impact VFX")]
        [SerializeField] private GameObject impactParticlesPrefab;
        [SerializeField] private float particlesLifeTime = 1.5f;

        [Header("Debug")]
        [SerializeField] private bool activateLogs = false;

        private void Start()
        {
            if (lifeTime > 0f) Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            int otherLayer = other.gameObject.layer;

            // 1) ¿Golpeó al Player?
            if (IsInLayerMask(otherLayer, playerMask))
            {
                if (activateLogs) Debug.Log("[Projectile] Player hit", this);

                var playerHealth = other.GetComponentInParent<HealthController>();
                if (!playerHealth) return;
                
                Vector3 impactPoint = other.ClosestPoint(transform.position);
                
                Vector3 toPlayer = (other.transform.position - impactPoint);
                Vector3 fakeOrigin = other.transform.position + toPlayer;

                playerHealth.Damage(new DamageInfo(
                    damage,
                    fakeOrigin,
                    (knockback.horizontal, knockback.vertical)
                ));


                SpawnImpactParticles();
                Destroy(gameObject);
                return;
            }

            // 2) ¿Golpeó entorno?
            if (IsInLayerMask(otherLayer, environmentMask))
            {
                if (activateLogs) Debug.Log("[Projectile] Environment hit", this);
                SpawnImpactParticles();
                Destroy(gameObject);
                return;
            }
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
            => (mask.value & (1 << layer)) != 0;

        private void SpawnImpactParticles()
        {
            if (!impactParticlesPrefab) return;
            var particles = Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
            if (particlesLifeTime > 0f) Destroy(particles, particlesLifeTime);
        }
    }
}
