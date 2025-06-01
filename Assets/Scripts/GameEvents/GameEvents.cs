using System;
using UnityEngine;

namespace GameEvents
{
    public static class GameEvents
    {
        public static event Action<GameObject> OnPlayerDied;
        public static event Action<bool> OnPlayerGodMode;
        public static event Action OnPlayerBlinded;

        public static void PlayerDied(GameObject player)
        {
            OnPlayerDied?.Invoke(player);
        }

        public static void PlayerGodMode(bool isPlayerGodModeActive)
        {
            OnPlayerGodMode?.Invoke(isPlayerGodModeActive);
        }

        public static void PlayerBlinded()
        {
            OnPlayerBlinded?.Invoke();
        }
    }
}