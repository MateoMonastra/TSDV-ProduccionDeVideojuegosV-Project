using System;
using UnityEngine;


public static class GameEvents
{
    public static event Action<GameObject> OnPlayerDied;

    public static void PlayerDied(GameObject player)
    {
        OnPlayerDied?.Invoke(player);
    }
}