using System;
using UnityEngine;

namespace Player.Old
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public event Action AnimationComplete;
        private void OnAnimationComplete()
        {
            AnimationComplete?.Invoke();
        }
    }
}