using UnityEngine;

namespace Coins
{
    public class CoinBurstSpawner : MonoBehaviour
    {
        [SerializeField] private CoinPickup coinPrefab;

        [Header("Burst")] [SerializeField] private int minCoins = 3;
        [SerializeField] private int maxCoins = 8;
        [SerializeField] private float spawnRadius = 0.25f;

        [Header("Force Random")] [SerializeField]
        private float minForceMultiplier = 0.8f;

        [SerializeField] private float maxForceMultiplier = 1.25f;

        public void SpawnCoins(Vector3 origin, int amountOverride = -1)
        {
            if (coinPrefab == null) return;

            int count = amountOverride > 0 ? amountOverride : Random.Range(minCoins, maxCoins + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = origin + Random.insideUnitSphere * spawnRadius;
                spawnPos.y = origin.y + Random.Range(4f, 5f);

                CoinPickup coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

                Vector3 dir = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;

                float forceMul = Random.Range(minForceMultiplier, maxForceMultiplier);
                coin.Launch(dir, forceMul);
            }
        }
    }
}