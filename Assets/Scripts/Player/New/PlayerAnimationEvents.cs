using System;
using UnityEngine;

namespace Player.New
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