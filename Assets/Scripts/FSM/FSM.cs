using UnityEngine;

namespace FSM
{
    public class Fsm
    {
        private State _current;

        public Fsm(State current)
        {
            _current = current;
            _current.Enter();
        }

        public void Update()
        {
            _current.Tick(Time.deltaTime);
        }

        public void FixedUpdate()
        {
            _current.FixedTick(Time.deltaTime);
        }

        /// <summary>
        /// Intenta transicionar usando un ID registrado en el estado actual.
        /// </summary>
        public bool TryTransitionTo(string id)
        {
            if (_current.TryGetTransition(id, out var transition))
            {
                Debug.Log($"FSM: transition {_current} -> {id} (via ID), success");
                transition.Do();                  // Exit -> OnTransition -> Enter
                _current = transition.To;
                return true;
            }

            Debug.Log($"FSM: transition {_current} -> {id} (via ID), failure");
            return false;
        }

        /// <summary>
        /// Transición forzada a un estado destino, sin requerir ID ni transición registrada.
        /// </summary>
        public void ForceTransition(State to)
        {
            if (to == null)
            {
                Debug.LogWarning("FSM: ForceTransition target is null");
                return;
            }
            if (ReferenceEquals(_current, to))
            {
                return;
            }
            
            var synthetic = new Transition
            {
                From = _current,
                To = to,
                ID = "FORCED" // sólo informativo
            };

            Debug.Log($"FSM: FORCE transition {_current} -> {to}");
            synthetic.Do();
            _current = to;
        }

        public State GetCurrentState() => _current;
    }
}
