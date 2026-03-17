using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject selectionFrame;

    private ItemData itemData;

    public ItemData ItemData => itemData;

    public void Setup(ItemData item, int amount, bool selected)
    {
        itemData = item;

        if (iconImage != null)
        {
            if (item != null && item.Icon != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = item.Icon;
            }
            else
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }
        }

        if (amountText != null)
        {
            amountText.text = amount.ToString();
        }

        if (selectionFrame != null)
        {
            selectionFrame.SetActive(selected);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(selected);
        }
    }
}