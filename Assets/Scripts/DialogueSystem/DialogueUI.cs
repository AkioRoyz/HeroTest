using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [System.Serializable]
    public class ChoiceViewData
    {
        public string Text;
        public bool IsSelectable;

        public ChoiceViewData(string text, bool isSelectable)
        {
            Text = text;
            IsSelectable = isSelectable;
        }
    }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Main UI")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Choices")]
    [SerializeField] private List<TMP_Text> choiceTexts = new();

    [Header("Selection Prefixes")]
    [SerializeField] private string selectedPrefix = "> ";
    [SerializeField] private string unselectedPrefix = "  ";
    [SerializeField] private string disabledPrefix = "X ";

    [Header("Disabled Choice Style")]
    [SerializeField] private bool tintDisabledChoices = true;
    [SerializeField] private Color normalChoiceColor = Color.white;
    [SerializeField] private Color disabledChoiceColor = Color.gray;

    [Header("Continue Indicator")]
    [Tooltip("Иконка, которая показывается только на обычных репликах без выбора.")]
    [SerializeField] private GameObject continueIndicatorObject;

    [Tooltip("RectTransform иконки. Нужен для плавного движения вверх-вниз.")]
    [SerializeField] private RectTransform continueIndicatorRect;

    [Tooltip("Насколько пикселей иконка поднимается вверх.")]
    [SerializeField] private float continueIndicatorMoveDistance = 10f;

    [Tooltip("Скорость движения иконки.")]
    [SerializeField] private float continueIndicatorMoveSpeed = 2f;

    private bool isContinueIndicatorVisible;
    private Vector2 continueIndicatorStartAnchoredPosition;

    private void Awake()
    {
        if (continueIndicatorRect != null)
        {
            continueIndicatorStartAnchoredPosition = continueIndicatorRect.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        ResetContinueIndicatorPosition();
    }

    private void Update()
    {
        UpdateContinueIndicatorAnimation();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        HideContinueIndicator();
    }

    public void SetSpeakerName(string speakerName)
    {
        if (speakerNameText != null)
            speakerNameText.text = speakerName ?? string.Empty;
    }

    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
            dialogueText.text = text ?? string.Empty;
    }

    public void SetPortrait(Sprite portrait)
    {
        if (portraitImage == null)
            return;

        if (portrait != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = true;
        }
        else
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }
    }

    public void ClearChoices()
    {
        for (int i = 0; i < choiceTexts.Count; i++)
        {
            if (choiceTexts[i] != null)
            {
                choiceTexts[i].gameObject.SetActive(false);
                choiceTexts[i].text = string.Empty;
                choiceTexts[i].color = normalChoiceColor;
            }
        }
    }

    public void SetChoices(List<ChoiceViewData> choices, int selectedIndex)
    {
        ClearChoices();

        if (choices == null)
            return;

        for (int i = 0; i < choiceTexts.Count; i++)
        {
            if (i >= choices.Count)
                break;

            TMP_Text choiceText = choiceTexts[i];
            if (choiceText == null)
                continue;

            choiceText.gameObject.SetActive(true);

            ChoiceViewData data = choices[i];

            bool isSelected = i == selectedIndex;
            string prefix;

            if (!data.IsSelectable)
            {
                prefix = disabledPrefix;
            }
            else
            {
                prefix = isSelected ? selectedPrefix : unselectedPrefix;
            }

            choiceText.text = prefix + data.Text;

            if (tintDisabledChoices)
            {
                choiceText.color = data.IsSelectable ? normalChoiceColor : disabledChoiceColor;
            }
            else
            {
                choiceText.color = normalChoiceColor;
            }
        }
    }

    public void ShowContinueIndicator()
    {
        isContinueIndicatorVisible = true;

        if (continueIndicatorObject != null)
        {
            continueIndicatorObject.SetActive(true);
        }

        ResetContinueIndicatorPosition();
    }

    public void HideContinueIndicator()
    {
        isContinueIndicatorVisible = false;

        if (continueIndicatorObject != null)
        {
            continueIndicatorObject.SetActive(false);
        }

        ResetContinueIndicatorPosition();
    }

    private void UpdateContinueIndicatorAnimation()
    {
        if (!isContinueIndicatorVisible)
            return;

        if (continueIndicatorRect == null)
            return;

        float offsetY = Mathf.Sin(Time.unscaledTime * continueIndicatorMoveSpeed) * continueIndicatorMoveDistance;
        continueIndicatorRect.anchoredPosition = continueIndicatorStartAnchoredPosition + new Vector2(0f, offsetY);
    }

    private void ResetContinueIndicatorPosition()
    {
        if (continueIndicatorRect == null)
            return;

        continueIndicatorRect.anchoredPosition = continueIndicatorStartAnchoredPosition;
    }
}