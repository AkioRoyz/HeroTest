using System;
using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    public static EquipmentSystem Instance;

    public event Action OnEquipmentChanged;

    [SerializeField] private StatsSystem statsSystem;
    [SerializeField] private int slotCount = 9;

    private ItemData[] equippedItems;

    public int SlotCount => slotCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        equippedItems = new ItemData[slotCount];
    }

    public ItemData GetItemInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedItems.Length)
            return null;

        return equippedItems[slotIndex];
    }

    public bool EquipItemToSlot(ItemData item, int slotIndex)
    {
        if (item == null)
        {
            Debug.LogWarning("EquipItemToSlot called with null item.");
            return false;
        }

        if (item.ItemType != ItemType.Equipment)
        {
            Debug.LogWarning($"{item.name} is not an equipment item.");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= equippedItems.Length)
        {
            Debug.LogWarning($"Invalid equipment slot index: {slotIndex}");
            return false;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("InventorySystem.Instance is missing.");
            return false;
        }

        // Сначала пытаемся забрать новый предмет из инвентаря
        bool removedFromInventory = InventorySystem.Instance.RemoveItem(item, 1);
        if (!removedFromInventory)
        {
            Debug.LogWarning($"Could not remove item from inventory: {item.ItemId}");
            return false;
        }

        ItemData oldItem = equippedItems[slotIndex];

        // Если в слоте что-то было — снимаем и возвращаем в инвентарь
        if (oldItem != null)
        {
            RemoveItemBonuses(oldItem);
            InventorySystem.Instance.AddItem(oldItem, 1);
        }

        equippedItems[slotIndex] = item;
        ApplyItemBonuses(item);

        OnEquipmentChanged?.Invoke();
        return true;
    }

    public bool UnequipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedItems.Length)
        {
            Debug.LogWarning($"Invalid equipment slot index: {slotIndex}");
            return false;
        }

        ItemData item = equippedItems[slotIndex];
        if (item == null)
            return false;

        RemoveItemBonuses(item);
        InventorySystem.Instance.AddItem(item, 1);
        equippedItems[slotIndex] = null;

        OnEquipmentChanged?.Invoke();
        return true;
    }

    private void ApplyItemBonuses(ItemData item)
    {
        if (statsSystem == null || item == null)
            return;

        statsSystem.AddBonusStats(
            item.EquipmentStrengthBonus,
            item.EquipmentManaBonus,
            item.EquipmentDefenceBonus
        );
    }

    private void RemoveItemBonuses(ItemData item)
    {
        if (statsSystem == null || item == null)
            return;

        statsSystem.RemoveBonusStats(
            item.EquipmentStrengthBonus,
            item.EquipmentManaBonus,
            item.EquipmentDefenceBonus
        );
    }
}