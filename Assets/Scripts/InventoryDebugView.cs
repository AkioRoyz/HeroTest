using UnityEngine;

public class InventoryDebugView : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;

    private void OnEnable()
    {
        inventorySystem.OnInventoryChanged += PrintInventory;
    }

    private void OnDisable()
    {
        inventorySystem.OnInventoryChanged -= PrintInventory;
    }

    private void PrintInventory()
    {
        Debug.Log("=== CONSUMABLES ===");
        for (int i = 0; i < inventorySystem.ConsumableItems.Count; i++)
        {
            InventoryEntry entry = inventorySystem.ConsumableItems[i];
            Debug.Log($"{entry.Item.ItemId} x{entry.Amount}");
        }

        Debug.Log("=== QUEST ITEMS ===");
        for (int i = 0; i < inventorySystem.QuestItems.Count; i++)
        {
            InventoryEntry entry = inventorySystem.QuestItems[i];
            Debug.Log($"{entry.Item.ItemId} x{entry.Amount}");
        }

        Debug.Log("=== EQUIPMENT ITEMS ===");
        for (int i = 0; i < inventorySystem.EquipmentItems.Count; i++)
        {
            InventoryEntry entry = inventorySystem.EquipmentItems[i];
            Debug.Log($"{entry.Item.ItemId} x{entry.Amount}");
        }
    }
}