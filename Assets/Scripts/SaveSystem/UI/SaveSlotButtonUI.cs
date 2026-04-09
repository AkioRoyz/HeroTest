using TMPro;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButtonUI : MonoBehaviour
{
    [SerializeField] private Button mainButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailsText;

    private Action onMainClick;
    private Action onDeleteClick;

    private void Awake()
    {
        if (mainButton != null)
            mainButton.onClick.AddListener(HandleMainButtonClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(HandleDeleteButtonClicked);
    }

    public void Configure(
        string title,
        string details,
        bool mainInteractable,
        bool showDeleteButton,
        Action onMainClick,
        Action onDeleteClick)
    {
        if (titleText != null)
            titleText.text = title;

        if (detailsText != null)
            detailsText.text = details;

        if (mainButton != null)
            mainButton.interactable = mainInteractable;

        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(showDeleteButton);
            deleteButton.interactable = showDeleteButton;
        }

        this.onMainClick = onMainClick;
        this.onDeleteClick = onDeleteClick;
    }

    private void HandleMainButtonClicked()
    {
        onMainClick?.Invoke();
    }

    private void HandleDeleteButtonClicked()
    {
        onDeleteClick?.Invoke();
    }
}