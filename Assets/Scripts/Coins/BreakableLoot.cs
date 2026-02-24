using UnityEngine;

namespace Coins
{
    public class BreakableLoot : MonoBehaviour
    {
        [SerializeField] private int hitsToBreak = 1;
        [SerializeField] private int coinAmount = 6;
        [SerializeField] private CoinBurstSpawner coinBurstSpawner;
        [SerializeField] private GameObject breakVfx;
        [SerializeField] private AudioSource breakSfx;

        private int _currentHits;

        private void Awake()
        {
            _currentHits = hitsToBreak;
        }

        public void Hit()
        {
            _currentHits--;

            if (_currentHits <= 0)
                Break();
        }

        private void Break()
        {
            if (breakVfx != null)
                Instantiate(breakVfx, transform.position, Quaternion.identity);

            if (breakSfx != null)
                breakSfx.Play();

            if (coinBurstSpawner != null)
                coinBurstSpawner.SpawnCoins(transform.position, coinAmount);

            Destroy(gameObject);
        }
    }
}