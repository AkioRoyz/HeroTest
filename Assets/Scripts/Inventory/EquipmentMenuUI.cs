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
    [SerializeField] private EquipmentInventoryTextListUI inventoryTextListUI;

    [Header("Slots UI")]
    [SerializeField] private EquipmentSlotUI[] slotUIs = new EquipmentSlotUI[9];

    [Header("Grid Settings")]
    [SerializeField] private int slotColumnCount = 3;

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
            gameInput.OnMenuLeft += HandleMenuLeft;
            gameInput.OnMenuRight += HandleMenuRight;
            gameInput.OnMenuSelect += HandleMenuSelect;
            gameInput.OnMenuUnequip += HandleMenuUnequip;
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
            gameInput.OnMenuLeft -= HandleMenuLeft;
            gameInput.OnMenuRight -= HandleMenuRight;
            gameInput.OnMenuSelect -= HandleMenuSelect;
            gameInput.OnMenuUnequip -= HandleMenuUnequip;
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
            MoveSlotSelection(-slotColumnCount);
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

            RefreshInventoryListVisualOnly();
            UpdateDescriptionFromCurrentSelection();
        }
    }

    private void HandleMenuDown()
    {
        if (currentMode == SelectionMode.Slots)
        {
            MoveSlotSelection(slotColumnCount);
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

            RefreshInventoryListVisualOnly();
            UpdateDescriptionFromCurrentSelection();
        }
    }

    private void HandleMenuLeft()
    {
        if (currentMode != SelectionMode.Slots)
            return;

        int row = selectedSlotIndex / slotColumnCount;
        int column = selectedSlotIndex % slotColumnCount;

        column--;
        if (column < 0)
        {
            column = slotColumnCount - 1;
        }

        selectedSlotIndex = row * slotColumnCount + column;
        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotUIs.Length - 1);

        RefreshSlotsUI();
        UpdateDescriptionFromCurrentSelection();
    }

    private void HandleMenuRight()
    {
        if (currentMode != SelectionMode.Slots)
            return;

        int row = selectedSlotIndex / slotColumnCount;
        int column = selectedSlotIndex % slotColumnCount;

        column++;
        if (column >= slotColumnCount)
        {
            column = 0;
        }

        selectedSlotIndex = row * slotColumnCount + column;
        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotUIs.Length - 1);

        RefreshSlotsUI();
        UpdateDescriptionFromCurrentSelection();
    }

    private void HandleMenuSelect()
    {
        if (currentMode == SelectionMode.Slots)
        {
            BuildInventoryEquipmentList();
            RebuildInventoryListUI();

            if (cachedEquipmentEntries.Count == 0)
            {
                RefreshAll();
                return;
            }

            selectedInventoryIndex = Mathf.Clamp(selectedInventoryIndex, 0, cachedEquipmentEntries.Count - 1);
            currentMode = SelectionMode.InventoryList;
            RefreshInventoryListVisualOnly();
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

    private void HandleMenuUnequip()
    {
        if (currentMode != SelectionMode.Slots)
            return;

        bool unequipped = equipmentSystem.UnequipSlot(selectedSlotIndex);
        if (unequipped)
        {
            RefreshAll();
        }
    }

    private void MoveSlotSelection(int delta)
    {
        int count = slotUIs.Length;
        selectedSlotIndex = (selectedSlotIndex + delta) % count;

        if (selectedSlotIndex < 0)
        {
            selectedSlotIndex += count;
        }

        RefreshSlotsUI();
        UpdateDescriptionFromCurrentSelection();
    }

    private void RefreshAll()
    {
        BuildInventoryEquipmentList();
        RefreshSlotsUI();
        RebuildInventoryListUI();
        RefreshInventoryListVisualOnly();
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

    private void RebuildInventoryListUI()
    {
        if (inventoryTextListUI == null)
            return;

        inventoryTextListUI.SetData(cachedEquipmentEntries);
    }

    private void RefreshInventoryListVisualOnly()
    {
        if (inventoryTextListUI == null)
            return;

        bool listIsActive = currentMode == SelectionMode.InventoryList;
        inventoryTextListUI.SetVisualState(listIsActive, selectedInventoryIndex);
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
        RefreshInventoryListVisualOnly();
        UpdateDescriptionFromCurrentSelection();
    }
}