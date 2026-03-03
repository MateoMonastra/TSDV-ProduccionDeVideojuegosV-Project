using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Coins
{
    public class CoinsWallet : MonoBehaviour
    {
        private static readonly int Enter = Animator.StringToHash("Enter");
        public static CoinsWallet Instance { get; private set; }

        [Header("UI")] [SerializeField] private TMP_Text coinsText;
        [SerializeField] private RectTransform coinIconTarget;

        [Header("UI Fly")] [SerializeField] private RectTransform rootCanvasRect;
        [SerializeField] private CoinUiFly coinUiFlyPrefab;

        [Header("Collect FX (Animator)")] [SerializeField]
        private GameObject collectFxGO;

        [SerializeField] private Animator collectFxAnimator;
        [SerializeField] private string playTriggerName = "Play";

        public int Coins { get; private set; }
        public RectTransform CoinIconTarget => coinIconTarget;

        private readonly Queue<Vector2> _pending = new Queue<Vector2>();
        private bool _fxPlaying;

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

        public void RequestCoinFromWorld(Vector3 worldPos, int amount)
        {
            if (rootCanvasRect == null || coinUiFlyPrefab == null || coinIconTarget == null)
            {
                AddCoins(amount);
                return;
            }

            // Camera cam = Camera.main;
            // Vector3 screenPos = cam != null
            //     ? cam.WorldToScreenPoint(worldPos)
            //     : RectTransformUtility.WorldToScreenPoint(null, worldPos);
            //
            // Vector2 anchored;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //     rootCanvasRect,
            //     screenPos,
            //     null, 
            //     out anchored
            // );
            //
            // for (int i = 0; i < Mathf.Max(1, amount); i++)
            //     _pending.Enqueue(anchored);

            EnsureFxPlaying();
        }

        private void EnsureFxPlaying()
        {
            if (_fxPlaying)
            {
                if (collectFxAnimator != null)
                    collectFxAnimator.ResetTrigger(playTriggerName);

                if (collectFxAnimator != null)
                    collectFxAnimator.SetTrigger(playTriggerName);

                AddCoins(1);
            }
            else
            {
                collectFxAnimator.SetTrigger(Enter);
            }

            _fxPlaying = true;
        }

        public void OnCollectFxPeak()
        {
            AddCoins(1);

            if (_pending.Count == 0)
                return;


            // Vector2 startAnchored = _pending.Dequeue();
            //
            // CoinUiFly fly = Instantiate(coinUiFlyPrefab, rootCanvasRect);
            // fly.Play(
            //     rootCanvasRect,
            //     startAnchored,
            //     coinIconTarget,
            //     () => AddCoins(1)
            // );
        }

        public void OnCollectFxFinished()
        {
            // if (_pending.Count > 0)
            // {
            //     if (collectFxAnimator != null)
            //         collectFxAnimator.SetTrigger(playTriggerName);
            // }
            // else
            // {
            _fxPlaying = false;
            //}
        }
    }
}