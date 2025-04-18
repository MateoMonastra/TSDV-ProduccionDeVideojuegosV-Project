using System;
using System.Collections.Generic;

namespace FSM
{
    public abstract class State
    {
        private List<Transition> _transitions = new();
        public Action OnEnter;
        public Action OnTick;
        public Action OnFixedTick;
        public Action OnExit;

        public virtual void Enter() => OnEnter?.Invoke();

        public virtual void Tick(float delta) => OnTick?.Invoke();

        public virtual void FixedTick(float delta) => OnFixedTick?.Invoke();

        public virtual void Exit() => OnExit?.Invoke();

        public bool TryGetTransition(string id, out Transition transition)
        {
            foreach (var transitionCandidate in _transitions)
            {
                if (transitionCandidate.ID == id)
                {
                    transition = transitionCandidate;
                    return true;
                }
            }

            transition = null;
            return false;
        }
        
        public void AddTransition(Transition transition) => _transitions.Add(transition);
    }
}