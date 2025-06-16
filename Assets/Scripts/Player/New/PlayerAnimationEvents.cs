using System;

namespace Player.New
{
    public class PlayerAnimationEvents
    {
        public event Action AnimationComplete;
        private void OnAnimationComplete()
        {
            AnimationComplete?.Invoke();
        }
    }
}