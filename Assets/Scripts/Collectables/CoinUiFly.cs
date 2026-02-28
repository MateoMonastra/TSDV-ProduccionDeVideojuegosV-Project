using System.Collections;
using UnityEngine;

namespace Coins
{
    public class CoinUiFly : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float arcHeight = 60f;

        private void Awake()
        {
            if (rect == null) rect = GetComponent<RectTransform>();
        }

        public void Play(RectTransform canvasRect, Vector2 startAnchoredPos, RectTransform target, System.Action onComplete)
        {
            if (rect == null) rect = GetComponent<RectTransform>();

            rect.SetParent(canvasRect, false);
            rect.anchoredPosition = startAnchoredPos;

            StartCoroutine(FlyRoutine(target, onComplete));
        }

        private IEnumerator FlyRoutine(RectTransform target, System.Action onComplete)
        {
            Vector2 start = rect.anchoredPosition;

            // Convertir target a anchoredPosition dentro del mismo canvas
            RectTransform parentRect = rect.parent as RectTransform;
            Vector2 targetAnchored;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                RectTransformUtility.WorldToScreenPoint(null, target.position), // Overlay => null
                null,
                out targetAnchored
            );

            float t = 0f;
            float d = Mathf.Max(0.01f, duration);

            while (t < 1f)
            {
                t += Time.deltaTime / d;
                float e = curve.Evaluate(Mathf.Clamp01(t));

                Vector2 p = Vector2.Lerp(start, targetAnchored, e);
                p.y += Mathf.Sin(e * Mathf.PI) * arcHeight;

                rect.anchoredPosition = p;
                yield return null;
            }

            onComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}