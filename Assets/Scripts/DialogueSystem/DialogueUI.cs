using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Main UI")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Choices")]
    [SerializeField] private List<TMP_Text> choiceTexts = new();

    [Header("Selection")]
    [SerializeField] private string selectedPrefix = "> ";
    [SerializeField] private string unselectedPrefix = "  ";

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
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
            }
        }
    }

    public void SetChoices(List<string> choices, int selectedIndex)
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

            bool isSelected = i == selectedIndex;
            string prefix = isSelected ? selectedPrefix : unselectedPrefix;
            choiceText.text = prefix + choices[i];
        }
    }
}