using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class InputReader : MonoBehaviour
    {
        public Action OnNavigate; 
        public Action OnPause;
        
        public void HandleNavigate(InputAction.CallbackContext context)
        {
            OnNavigate?.Invoke();
        }
        
        public void HandlePauseInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnPause?.Invoke();
            }
        }
    }
}
