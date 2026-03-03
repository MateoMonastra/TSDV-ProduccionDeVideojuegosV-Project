using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Player
{
    public class InputReader : MonoBehaviour
    {
        public Action OnNavigate;
        public Action OnClick;
        public Action OnPause;
        public Action OnJump;
        public Action OnInteract;
        public Action<Vector2> OnMove;
        public Action<Vector2> OnFlyMove;
        public Action<Vector2,InputDevice> OnLook;
        public Action OnFlyUp;
        public Action OnFlyUpCanceled;
        public Action OnFlyDown;
        public Action OnFlyDownCanceled;
        public Action OnGodModeCheat;
        public Action OnJumpPickUpCheat;
        public Action OnDashPickUpCheat;
        public Action OnDash;
        public Action OnAttackHeavyPressed;
        public Action OnAttackHeavyReleased;
        public Action<bool> OnDashHeldChanged;
        public Action<InputDevice> OnInputPressed;

        
        [SerializeField] private string showRoomSceneName;
        [SerializeField] private string levelSceneName;
        [SerializeField] private string pipesSceneName;
        [SerializeField] private string milestone4SceneName;
        
        public void HandleNavigate(InputAction.CallbackContext context)
        {
            OnNavigate?.Invoke();
            OnInputPressed?.Invoke(context.control.device);
        }
        
        public void HandleClick(InputAction.CallbackContext context)
        {
            if(context.started)
                OnClick?.Invoke();
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandlePauseInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnPause?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }
        public void HandleJumpInput(InputAction.CallbackContext context)
        {
            if (SceneManager.GetActiveScene().name == "SplashScene")
            {
                SceneManager.LoadScene(levelSceneName);
                Time.timeScale = 1;
            }
            
            if (context.started)
            {
                OnJump?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleInteractInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnInteract?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }
        
        public void HandleLookInput(InputAction.CallbackContext context)
        {
            OnLook?.Invoke(context.ReadValue<Vector2>(), context.control.device);
            
            OnInputPressed?.Invoke(context.control.device);
        }


        public void HandleMoveInput(InputAction.CallbackContext context)
        {
            OnMove?.Invoke(context.ReadValue<Vector2>());
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleFlyMoveInput(InputAction.CallbackContext context)
        {
            OnFlyMove?.Invoke(context.ReadValue<Vector2>());
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleFlyUpInput(InputAction.CallbackContext context)
        {
            OnFlyUp?.Invoke();

            if (context.canceled)
            {
                OnFlyUpCanceled?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleFlyDownInput(InputAction.CallbackContext context)
        {
            OnFlyDown?.Invoke();

            if (context.canceled)
            {
                OnFlyDownCanceled?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleShowroomButton()
        {
            return;
            
            SceneManager.LoadScene("SplashScene");
            Time.timeScale = 1;
        }
        
        public void HandleShowRoomInput(InputAction.CallbackContext context)
        {
            return;
            //SceneManager.LoadScene("SplashScene");
            //Time.timeScale = 1;
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleLevelInput(InputAction.CallbackContext context)
        {
            return;
            
            SceneManager.LoadScene(levelSceneName);
            Time.timeScale = 1;
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandlePipesInput(InputAction.CallbackContext context)
        {
            // SceneManager.LoadScene(pipesSceneName);
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleMilestone4Input(InputAction.CallbackContext context)
        {
            // SceneManager.LoadScene(milestone4SceneName);
            
            OnInputPressed?.Invoke(context.control.device);
        }
        
        public void HandleGodModeInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnGodModeCheat?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleDashCheatInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnDashPickUpCheat?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleJumpCheatInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnJumpPickUpCheat?.Invoke();
            }
            
            OnInputPressed?.Invoke(context.control.device);
        }
        public void HandleDashInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnDash?.Invoke();
                OnDashHeldChanged?.Invoke(true);
            }

            if (context.canceled)  OnDashHeldChanged?.Invoke(false);
            
            OnInputPressed?.Invoke(context.control.device);
        }

        public void HandleAttackHeavyInput(InputAction.CallbackContext context)
        {
            if (context.started) OnAttackHeavyPressed?.Invoke();
            if (context.canceled) OnAttackHeavyReleased?.Invoke();
            
            OnInputPressed?.Invoke(context.control.device);
        }
    }
}
