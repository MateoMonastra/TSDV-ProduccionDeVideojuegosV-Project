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
        
        [SerializeField] private string showRoomSceneName;
        [SerializeField] private string levelSceneName;

        public void HandleNavigate(InputAction.CallbackContext context)
        {
            OnNavigate?.Invoke();
        }
        
        public void HandleClick(InputAction.CallbackContext context)
        {
            if(context.started)
                OnClick?.Invoke();
        }

        public void HandlePauseInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnPause?.Invoke();
            }
        }
        public void HandleJumpInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnJump?.Invoke();
            }
        }
        
        public void HandleLookInput(InputAction.CallbackContext context)
        {
            OnLook?.Invoke(context.ReadValue<Vector2>(), context.control.device);
        }


        public void HandleMoveInput(InputAction.CallbackContext context)
        {
            OnMove?.Invoke(context.ReadValue<Vector2>());
        }

        public void HandleFlyMoveInput(InputAction.CallbackContext context)
        {
            OnFlyMove?.Invoke(context.ReadValue<Vector2>());
        }

        public void HandleFlyUpInput(InputAction.CallbackContext context)
        {
            OnFlyUp?.Invoke();

            if (context.canceled)
            {
                OnFlyUpCanceled?.Invoke();
            }
        }

        public void HandleFlyDownInput(InputAction.CallbackContext context)
        {
            OnFlyDown?.Invoke();

            if (context.canceled)
            {
                OnFlyDownCanceled?.Invoke();
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

        public void HandleGodModeInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnGodModeCheat?.Invoke();
            }
        }

        public void HandleDashCheatInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnDashPickUpCheat?.Invoke();
            }
        }

        public void HandleJumpCheatInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnJumpPickUpCheat?.Invoke();
            }
        }
        public void HandleDashInput(InputAction.CallbackContext context)
        {
            if (context.started) OnDash?.Invoke();
        }
        public void HandleAttackHeavyInput(InputAction.CallbackContext context)
        {
            if (context.started) OnAttackHeavyPressed?.Invoke();
            if (context.canceled) OnAttackHeavyReleased?.Invoke();
        }
    }
}
