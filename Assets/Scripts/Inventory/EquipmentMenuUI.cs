using System.Collections.Generic;
using UnityEngine;

public class EquipmentMenuUI : MonoBehaviour
{
    private enum SelectionMode
    {
        Slots,
        InventoryList
    }

    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private EquipmentSystem equipmentSystem;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private ItemDescriptionPanelUI descriptionPanel;

    [Header("Slots UI")]
    [SerializeField] private EquipmentSlotUI[] slotUIs = new EquipmentSlotUI[9];

    [Header("Inventory List UI")]
    [SerializeField] private Transform inventoryListContainer;
    [SerializeField] private EquipmentInventoryItemUI inventoryItemPrefab;

    private readonly List<EquipmentInventoryItemUI> spawnedInventoryItems = new();
    private readonly List<InventoryEntry> cachedEquipmentEntries = new();

    private int selectedSlotIndex;
    private int selectedInventoryIndex;
    private SelectionMode currentMode = SelectionMode.Slots;

    private void Start()
    {
        SetupSlots();
        RefreshAll();
        SetMenuToSlotsMode();
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnMenuUp += HandleMenuUp;
            gameInput.OnMenuDown += HandleMenuDown;
            gameInput.OnMenuSelect += HandleMenuSelect;
        }

        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged += RefreshAll;
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged += RefreshAll;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnMenuUp -= HandleMenuUp;
            gameInput.OnMenuDown -= HandleMenuDown;
            gameInput.OnMenuSelect -= HandleMenuSelect;
        }

        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged -= RefreshAll;
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= RefreshAll;
        }
    }

    public void OpenMenu()
    {
        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotUIs.Length - 1);
        selectedInventoryIndex = 0;
        SetMenuToSlotsMode();
        RefreshAll();
    }

    private void SetupSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
            {
                slotUIs[i].Setup(i);
            }
        }
    }

    private void HandleMenuUp()
    {
        if (currentMode == SelectionMode.Slots)
        {
            selectedSlotIndex--;
            if (selectedSlotIndex < 0)
            {
                selectedSlotIndex = slotUIs.Length - 1;
            }

            RefreshSlotsUI();
            UpdateDescriptionFromCurrentSelection();
        }
        else
        {
            if (cachedEquipmentEntries.Count == 0)
                return;

            selectedInventoryIndex--;
            if (selectedInventoryIndex < 0)
            {
                selectedInventoryIndex = cachedEquipmentEntries.Count - 1;
            }

            RefreshInventoryListUI();
            UpdateDescriptionFromCurrentSelection();
        }
    }

    private void HandleMenuDown()
    {
        if (currentMode == SelectionMode.Slots)
        {
            selectedSlotIndex++;
            if (selectedSlotIndex >= slotUIs.Length)
            {
                selectedSlotIndex = 0;
            }

            RefreshSlotsUI();
            UpdateDescriptionFromCurrentSelection();
        }
        else
        {
            if (cachedEquipmentEntries.Count == 0)
                return;

            selectedInventoryIndex++;
            if (selectedInventoryIndex >= cachedEquipmentEntries.Count)
            {
                selectedInventoryIndex = 0;
            }

            RefreshInventoryListUI();
            UpdateDescriptionFromCurrentSelection();
        }
    }

    private void HandleMenuSelect()
    {
        if (currentMode == SelectionMode.Slots)
        {
            BuildInventoryEquipmentList();

            if (cachedEquipmentEntries.Count == 0)
            {
                // Если предметов нет, можно использовать Select ещё раз для снятия предмета
                bool unequipped = equipmentSystem.UnequipSlot(selectedSlotIndex);
                if (unequipped)
                {
                    RefreshAll();
                }
                return;
            }

            selectedInventoryIndex = 0;
            currentMode = SelectionMode.InventoryList;
            RefreshInventoryListUI();
            UpdateDescriptionFromCurrentSelection();
        }
        else
        {
            if (cachedEquipmentEntries.Count == 0)
            {
                SetMenuToSlotsMode();
                RefreshAll();
                return;
            }

            InventoryEntry chosenEntry = cachedEquipmentEntries[selectedInventoryIndex];

            if (chosenEntry != null && chosenEntry.Item != null)
            {
                bool equipped = equipmentSystem.EquipItemToSlot(chosenEntry.Item, selectedSlotIndex);

                if (equipped)
                {
                    SetMenuToSlotsMode();
                    RefreshAll();
                }
            }
        }
    }

    private void RefreshAll()
    {
        BuildInventoryEquipmentList();
        RefreshSlotsUI();
        RefreshInventoryListUI();
        UpdateDescriptionFromCurrentSelection();
    }

    private void RefreshSlotsUI()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
                continue;

            ItemData equippedItem = equipmentSystem.GetItemInSlot(i);
            bool selected = currentMode == SelectionMode.Slots && i == selectedSlotIndex;
            slotUIs[i].Refresh(equippedItem, selected);
        }
    }

    private void RefreshInventoryListUI()
    {
        RebuildInventoryListVisuals();

        for (int i = 0; i < spawnedInventoryItems.Count; i++)
        {
            bool selected = currentMode == SelectionMode.InventoryList && i == selectedInventoryIndex;
            spawnedInventoryItems[i].SetSelected(selected);
        }
    }

    private void BuildInventoryEquipmentList()
    {
        cachedEquipmentEntries.Clear();

        if (inventorySystem == null)
            return;

        for (int i = 0; i < inventorySystem.EquipmentItems.Count; i++)
        {
            InventoryEntry entry = inventorySystem.EquipmentItems[i];
            if (entry != null && entry.Item != null && entry.Amount > 0)
            {
                cachedEquipmentEntries.Add(entry);
            }
        }

        if (selectedInventoryIndex >= cachedEquipmentEntries.Count)
        {
            selectedInventoryIndex = Mathf.Max(0, cachedEquipmentEntries.Count - 1);
        }
    }

    private void RebuildInventoryListVisuals()
    {
        for (int i = 0; i < spawnedInventoryItems.Count; i++)
        {
            if (spawnedInventoryItems[i] != null)
            {
                Destroy(spawnedInventoryItems[i].gameObject);
            }
        }

        spawnedInventoryItems.Clear();

        if (inventoryItemPrefab == null || inventoryListContainer == null)
            return;

        for (int i = 0; i < cachedEquipmentEntries.Count; i++)
        {
            InventoryEntry entry = cachedEquipmentEntries[i];

            EquipmentInventoryItemUI itemUI = Instantiate(inventoryItemPrefab, inventoryListContainer);
            bool selected = currentMode == SelectionMode.InventoryList && i == selectedInventoryIndex;
            itemUI.Setup(entry.Item, entry.Amount, selected);

            spawnedInventoryItems.Add(itemUI);
        }
    }

    private void UpdateDescriptionFromCurrentSelection()
    {
        if (descriptionPanel == null)
            return;

        if (currentMode == SelectionMode.Slots)
        {
            ItemData equippedItem = equipmentSystem.GetItemInSlot(selectedSlotIndex);

            if (equippedItem == null)
            {
                descriptionPanel.Clear();
            }
            else
            {
                descriptionPanel.ShowItem(equippedItem);
            }
        }
        else
        {
            if (cachedEquipmentEntries.Count == 0)
            {
                descriptionPanel.Clear();
                return;
            }

            if (selectedInventoryIndex < 0 || selectedInventoryIndex >= cachedEquipmentEntries.Count)
            {
                descriptionPanel.Clear();
                return;
            }

            InventoryEntry entry = cachedEquipmentEntries[selectedInventoryIndex];

            if (entry == null || entry.Item == null)
            {
                descriptionPanel.Clear();
            }
            else
            {
                descriptionPanel.ShowItem(entry.Item);
            }
        }
    }

    private void SetMenuToSlotsMode()
    {
        currentMode = SelectionMode.Slots;
        RefreshSlotsUI();
        RefreshInventoryListUI();
        UpdateDescriptionFromCurrentSelection();
    }
}