using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Player
{
    public class InputReader : MonoBehaviour
    {
        public Action OnNavigate; 
        public Action OnPause;
        
        [SerializeField] private string showRoomSceneName;
        [SerializeField] private string levelSceneName;
        
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

        public void HandleShowRoomInput(InputAction.CallbackContext context)
        {
            SceneManager.LoadScene(showRoomSceneName);
            Time.timeScale = 1;
        }
        
        public void HandleLevelInput(InputAction.CallbackContext context)
        {
            SceneManager.LoadScene(levelSceneName);
            Time.timeScale = 1;
        }
    }
}
