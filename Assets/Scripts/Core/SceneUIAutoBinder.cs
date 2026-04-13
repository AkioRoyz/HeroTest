using System.Collections.Generic;
using UnityEngine;

public sealed class SceneUIAutoBinder : MonoBehaviour
{
    private readonly List<IPlayerBindable> bindables = new();

    private void OnEnable()
    {
        CacheBindables();
        PlayerRegistry.OnPlayerChanged += HandlePlayerChanged;
        HandlePlayerChanged(PlayerRegistry.CurrentPlayer);
    }

    private void OnDisable()
    {
        PlayerRegistry.OnPlayerChanged -= HandlePlayerChanged;
        UnbindAll();
    }

    private void CacheBindables()
    {
        bindables.Clear();

        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPlayerBindable bindable)
                bindables.Add(bindable);
        }
    }

    private void HandlePlayerChanged(GameObject playerRoot)
    {
        UnbindAll();

        if (playerRoot == null)
            return;

        for (int i = 0; i < bindables.Count; i++)
            bindables[i].BindPlayer(playerRoot);
    }

    private void UnbindAll()
    {
        for (int i = 0; i < bindables.Count; i++)
            bindables[i].UnbindPlayer();
    }
}