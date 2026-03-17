using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private SlidesManager slidesManager;
        [SerializeField] private GameObject playerStats;
        [SerializeField] private string menuSceneName;
        private void OnEnable()
        {
            inputReader.OnPause += InitPauseMenu;
        }

        private void OnDisable()
        {
            inputReader.OnPause -= InitPauseMenu;
        }
    
        private void InitPauseMenu()
        {
            GameEvents.GameEvents.GamePaused(true);
            slidesManager.gameObject.SetActive(true);
            playerStats.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
    
        public void Return()
        {
            GameEvents.GameEvents.GamePaused(false);
            playerStats.SetActive(true);
            slidesManager.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    
        public void ExitGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
