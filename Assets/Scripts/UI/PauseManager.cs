using Player;
using UnityEngine;

namespace UI
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private SlidesManager slidesManager;
        private void Start()
        {
            inputReader.OnPause += InitPauseMenu;
        }
    
        private void InitPauseMenu()
        {
            slidesManager.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
    
        public void Return()
        {
            slidesManager.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    
        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
