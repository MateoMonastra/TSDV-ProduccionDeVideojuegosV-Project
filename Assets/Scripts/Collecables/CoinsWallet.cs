using TMPro;
using UnityEngine;

namespace Coins
{
    public class CoinsWallet : MonoBehaviour
    {
        public static CoinsWallet Instance { get; private set; }

        [Header("UI")] [SerializeField] private TMP_Text coinsText;
        [SerializeField] private RectTransform coinIconTarget;

        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private RectTransform rootCanvasRect;
        [SerializeField] private CoinUiFly coinUiFlyPrefab;

        public Canvas RootCanvas => rootCanvas;
        public RectTransform RootCanvasRect => rootCanvasRect;
        public CoinUiFly CoinUiFlyPrefab => coinUiFlyPrefab;
        public int Coins { get; private set; }

        public RectTransform CoinIconTarget => coinIconTarget;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            RefreshUI();
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (coinsText != null)
                coinsText.text = Coins.ToString();
        }
    }
}