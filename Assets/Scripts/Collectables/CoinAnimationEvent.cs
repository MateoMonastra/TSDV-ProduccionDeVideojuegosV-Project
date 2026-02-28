using UnityEngine;

namespace Coins
{
    public class CoinAnimationEvent : MonoBehaviour
    {
        public void OnPeak()
        {
            if (CoinsWallet.Instance != null)
                CoinsWallet.Instance.OnCollectFxPeak();
        }
        
        public void OnFinished()
        {
            if (CoinsWallet.Instance != null)
                CoinsWallet.Instance.OnCollectFxFinished();
        }
    }
}