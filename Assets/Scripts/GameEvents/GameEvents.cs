using System;
using UnityEngine;

namespace GameEvents
{
    public static class GameEvents
    {
        public static event Action<GameObject> OnPlayerDied;
        public static event Action OnPlayerDamaged;
        public static event Action<bool> OnPlayerGodMode;
        public static event Action<bool> OnGamePaused;
        public static event Action OnPlayerBlinded;

        public static void PlayerDied(GameObject player)
        {
            OnPlayerDied?.Invoke(player);
        }
        
        public static void PlayerDamaged()
        {
            OnPlayerDamaged?.Invoke();
        }

        public static void PlayerGodMode(bool isPlayerGodModeActive)
        {
            OnPlayerGodMode?.Invoke(isPlayerGodModeActive);
        }

        public static void PlayerBlinded()
        {
            OnPlayerBlinded?.Invoke();
        }

        public static void GamePaused(bool isGamePaused)
        {
            OnGamePaused?.Invoke(isGamePaused);   
        }
    }
}