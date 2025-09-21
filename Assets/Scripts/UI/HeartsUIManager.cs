using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manager de corazones enteros (Empty/Full) sin "max".
    /// Usa hearts.Length como capacidad.
    /// hearts[0] debe ser el corazón más a la IZQUIERDA; hearts[^1], el de la DERECHA.
    /// </summary>
    public class HeartsUIManager : MonoBehaviour
    {
        [SerializeField] private HeartWidget[] hearts;

        [Header("Animación")]
        [SerializeField, Tooltip("Si está activo, aplica los cambios uno por uno.")]
        private bool rtlSequential = false;

        [SerializeField, Range(0.01f, 0.25f)]
        private float rtlStepDelay = 0.06f;

        private int _prevCount = -1;
        private Coroutine _seqCo;

        /// <summary>Inicializa el UI con la vida actual.</summary>
        public void Initialize(int current)
        {
            _prevCount = -1;
            SetHearts(current);
        }

        /// <summary>
        /// Actualiza la vida en corazones llenos.
        /// No usa "max": la capacidad es hearts.Length.
        /// </summary>
        public void SetHearts(int current)
        {
            int capacity = (hearts != null) ? hearts.Length : 0;
            if (capacity == 0) return;

            current = Mathf.Clamp(current, 0, capacity);

            if (_prevCount < 0)
            {
                for (int i = 0; i < capacity; i++)
                {
                    var state = (i < current) ? HeartState.Full : HeartState.Empty;
                    if (hearts != null) hearts[i]?.Init(state);
                }
                _prevCount = current;
                return;
            }

            if (_seqCo != null) { StopCoroutine(_seqCo); _seqCo = null; }

            if (!rtlSequential)
            {
                ApplyImmediate(current, capacity);
            }
            else
            {
                _seqCo = StartCoroutine(ApplySequential(current, capacity));
            }
        }

        // ========================= Helpers =========================

        private void ApplyImmediate(int current, int capacity)
        {
            if (current < _prevCount)
            {
                for (int i = _prevCount - 1; i >= current; i--)
                    hearts[i]?.Play(HeartState.Empty, TransitionCause.Damage);
            }
            else if (current > _prevCount)
            {
                for (int i = _prevCount; i < current; i++)
                    hearts[i]?.Play(HeartState.Full, TransitionCause.Heal);
            }

            _prevCount = current;
        }

        private IEnumerator ApplySequential(int current, int capacity)
        {
            if (current < _prevCount)
            {
                for (int i = _prevCount - 1; i >= current; i--)
                {
                    hearts[i]?.Play(HeartState.Empty, TransitionCause.Damage);
                    yield return new WaitForSecondsRealtime(rtlStepDelay);
                }
            }
            else if (current > _prevCount)
            {
                for (int i = _prevCount; i < current; i++)
                {
                    hearts[i]?.Play(HeartState.Full, TransitionCause.Heal);
                    yield return new WaitForSecondsRealtime(rtlStepDelay);
                }
            }

            _prevCount = current;
            _seqCo = null;
        }
    }
}
