using Player;
using UnityEngine;

namespace UI
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private SlidesManager slidesManager;
        [SerializeField] private GameObject playerStats;
        private void Start()
        {
            inputReader.OnPause += InitPauseMenu;
        }
    
        private void InitPauseMenu()
        {
            slidesManager.gameObject.SetActive(true);
            playerStats.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
    
        public void Return()
        {
            playerStats.SetActive(true);
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
