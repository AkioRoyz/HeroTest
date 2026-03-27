using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickConsumableBarUI : MonoBehaviour
{
    [System.Serializable]
    public class QuickSlotUI
    {
        public ItemData Item;
        public Image IconImage;
        public TextMeshProUGUI CountText;
    }

    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private QuickSlotUI[] slots = new QuickSlotUI[5];

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            QuickSlotUI slot = slots[i];

            if (slot == null || slot.IconImage == null || slot.CountText == null)
                continue;

            if (slot.Item == null)
            {
                slot.IconImage.enabled = false;
                slot.CountText.text = "0";
                continue;
            }

            slot.IconImage.enabled = true;
            slot.IconImage.sprite = slot.Item.Icon;

            int count = inventorySystem.GetItemCount(slot.Item);
            slot.CountText.text = count.ToString();

            Color iconColor = slot.IconImage.color;
            iconColor.a = count > 0 ? 1f : 0.3f;
            slot.IconImage.color = iconColor;
        }
    }
}