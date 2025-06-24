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

        public bool TryTransitionTo(string id)
        {
            if (_current.TryGetTransition(id, out var transition))
            {
                Debug.Log($"transition {_current} to {id}, success");
                transition.Do();
                _current = transition.To;
                return true;
            }

            Debug.Log($"transition {_current} to {id}, failure");
            return false;
        }

        public State GetCurrentState()
        {
            return _current;
        }

        public void ForceSetCurrentState(State state)
        {
            _current = state;
            _current.Enter();
        }
    }
}