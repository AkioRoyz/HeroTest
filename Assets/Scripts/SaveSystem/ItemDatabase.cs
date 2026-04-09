using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Save System/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemData[] items;

    private Dictionary<string, ItemData> lookup;

    public IReadOnlyList<ItemData> Items => items;

    public ItemData GetItemById(string itemId)
    {
        BuildLookupIfNeeded();

        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        lookup.TryGetValue(itemId, out ItemData item);
        return item;
    }

    public bool Contains(string itemId)
    {
        return GetItemById(itemId) != null;
    }

    private void OnEnable()
    {
        BuildLookupIfNeeded(true);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        lookup = null;
    }
#endif

    private void BuildLookupIfNeeded(bool forceRebuild = false)
    {
        if (!forceRebuild && lookup != null)
            return;

        lookup = new Dictionary<string, ItemData>();

        if (items == null)
            return;

        for (int i = 0; i < items.Length; i++)
        {
            ItemData item = items[i];
            if (item == null)
                continue;

            if (string.IsNullOrWhiteSpace(item.ItemId))
            {
                Debug.LogWarning($"[ItemDatabase] Item '{item.name}' has empty ItemId.", item);
                continue;
            }

            if (lookup.ContainsKey(item.ItemId))
            {
                Debug.LogWarning($"[ItemDatabase] Duplicate ItemId found: '{item.ItemId}'. Last one wins.", item);
                lookup[item.ItemId] = item;
                continue;
            }

            lookup.Add(item.ItemId, item);
        }
    }
}