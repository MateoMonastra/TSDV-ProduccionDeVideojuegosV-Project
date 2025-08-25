using System.Linq;
using UnityEngine;

namespace FSM
{
    public class Fsm
    {
        private State _current;
        
        public State Current => _current;
        public State LastFrom { get; private set; }
        public State LastTo { get; private set; }
        public string LastTransitionId { get; private set; }
        public bool LastTransitionSucceeded { get; private set; }
        
        public event System.Action<State, string, bool, State> OnAfterTransitionAttempt;

        public Fsm(State current)
        {
            _current = current;
            _current.Enter();
        }

        public void Update()      => _current.Tick(Time.deltaTime);
        public void FixedUpdate() => _current.FixedTick(Time.fixedDeltaTime);

        /// <summary>
        /// Intenta transicionar por ID. Si falla, loguea el listado de IDs disponibles
        /// </summary>
        public bool TryTransitionTo(string id)
        {
            return TryTransitionTo(id, warnIfMissing: true);
        }

        /// <summary>
        /// Versión con control de logging.
        /// </summary>
        public bool TryTransitionTo(string id, bool warnIfMissing)
        {
            LastFrom = _current;
            LastTransitionId = id;

            if (_current.TryGetTransition(id, out var transition))
            {
                Debug.Log($"FSM: transition {LastFrom} -> {id} (success)");
                transition.Do();
                _current = transition.To;

                LastTo = _current;
                LastTransitionSucceeded = true;
                OnAfterTransitionAttempt?.Invoke(LastFrom, id, true, LastTo);
                return true;
            }

            LastTo = null;
            LastTransitionSucceeded = false;

            if (warnIfMissing)
            {
                var ids = (_current is ITransitionsDebug dbg && dbg.GetTransitionIds() != null)
                    ? string.Join(", ", dbg.GetTransitionIds())
                    : "(no expuesto)";

                Debug.LogWarning(
                    $"FSM: transition {LastFrom} -> \"{id}\" (FAIL). " +
                    $"IDs disponibles desde {LastFrom}: {ids}"
                );
            }

            OnAfterTransitionAttempt?.Invoke(LastFrom, id, false, null);
            return false;
        }

        /// <summary>
        /// Transición forzada a un estado particular (sin ID). Útil para mecánicas
        /// que deben cortar lo que esté pasando.
        /// </summary>
        public void ForceTransition(State to)
        {
            if (to == null)
            {
                Debug.LogWarning("FSM: ForceTransition target is null");
                return;
            }
            if (ReferenceEquals(_current, to))
                return;

            var synthetic = new Transition
            {
                From = _current,
                To   = to,
                ID   = "FORCED"
            };

            LastFrom = _current;
            LastTransitionId = synthetic.ID;

            Debug.Log($"FSM: FORCE transition {LastFrom} -> {to}");
            synthetic.Do();
            _current = to;

            LastTo = _current;
            LastTransitionSucceeded = true;
            OnAfterTransitionAttempt?.Invoke(LastFrom, LastTransitionId, true, LastTo);
        }

        public State GetCurrentState()
        {
            return _current;
        }
    }
}
