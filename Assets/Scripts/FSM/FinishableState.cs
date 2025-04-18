using System;

namespace FSM
{
    public abstract class FinishableState : State
    {
        public event Action OnFinish;

        public virtual void Finish() => OnFinish?.Invoke();
    }
}