using System;

namespace FSM
{
    public class Transition
    {
        public State From { get; set; }
        public State To {  get; set; }
        public string ID {  get; set; }

        public event Action OnTransition;

        public void Do()
        {
            From.Exit();
            OnTransition?.Invoke();
            To.Enter();
        }
    }
}

