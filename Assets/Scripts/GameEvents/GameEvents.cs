using System;
using UnityEngine;

namespace GameEvents
{
    public static class GameEvents
    {
        public static event Action OnLevelStarted;
        public static event Action OnLevelEnded;
        public static event Action OnPlayerDied;
        public static event Action OnPlayerRevived;
        public static event Action OnPlayerDamaged;
        public static event Action<bool> OnPlayerGodMode;
        public static event Action<bool> OnGamePaused;
        public static event Action OnPlayerBlinded;

        public static void LevelStarted()
        {
            OnLevelStarted?.Invoke();
        }

        public static void LevelEnded()
        {
            OnLevelEnded?.Invoke();
        }

        public static void PlayerDied()
        {
            OnPlayerDied?.Invoke();
        }
        public static void PlayerRevived()
        {
            OnPlayerRevived?.Invoke();
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