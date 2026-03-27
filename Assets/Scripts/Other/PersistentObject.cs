using System.Collections.Generic;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    public enum DuplicateHandlingMode
    {
        DestroyNewest,
        DestroyOldest,
        AllowMultiple
    }

    private static readonly Dictionary<string, PersistentObject> ActiveObjectsById = new();

    [Header("Persistence")]
    [Tooltip("Уникальный ID для этого объекта. Если оставить пустым, будет использоваться имя GameObject, но лучше заполнять вручную.")]
    [SerializeField] private string persistentId = "";

    [Tooltip("Как вести себя, если при загрузке новой сцены найден дубликат с тем же ID.")]
    [SerializeField] private DuplicateHandlingMode duplicateHandlingMode = DuplicateHandlingMode.DestroyNewest;

    private string runtimeId;
    private bool isRegistered;

    public string PersistentId => runtimeId;

    private void Awake()
    {
        runtimeId = string.IsNullOrWhiteSpace(persistentId)
            ? gameObject.name
            : persistentId.Trim();

        if (string.IsNullOrWhiteSpace(runtimeId))
        {
            Debug.LogWarning($"{name}: PersistentObject has empty runtimeId. Object name will be used instead.");
            runtimeId = gameObject.name;
        }

        if (duplicateHandlingMode != DuplicateHandlingMode.AllowMultiple &&
            ActiveObjectsById.TryGetValue(runtimeId, out PersistentObject existing) &&
            existing != null &&
            existing != this)
        {
            switch (duplicateHandlingMode)
            {
                case DuplicateHandlingMode.DestroyNewest:
                    Destroy(gameObject);
                    return;

                case DuplicateHandlingMode.DestroyOldest:
                    ActiveObjectsById.Remove(runtimeId);

                    if (existing != null)
                    {
                        Destroy(existing.gameObject);
                    }
                    break;
            }
        }

        ActiveObjectsById[runtimeId] = this;
        isRegistered = true;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (!isRegistered)
            return;

        if (string.IsNullOrWhiteSpace(runtimeId))
            return;

        if (ActiveObjectsById.TryGetValue(runtimeId, out PersistentObject current) && current == this)
        {
            ActiveObjectsById.Remove(runtimeId);
        }
    }
}