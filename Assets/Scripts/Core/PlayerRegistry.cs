using System;
using UnityEngine;

public static class PlayerRegistry
{
    public static GameObject CurrentPlayer { get; private set; }

    public static event Action<GameObject> OnPlayerChanged;

    public static void Register(GameObject player)
    {
        if (CurrentPlayer == player)
            return;

        CurrentPlayer = player;
        OnPlayerChanged?.Invoke(CurrentPlayer);
    }

    public static void Unregister(GameObject player)
    {
        if (CurrentPlayer != player)
            return;

        CurrentPlayer = null;
        OnPlayerChanged?.Invoke(null);
    }
}