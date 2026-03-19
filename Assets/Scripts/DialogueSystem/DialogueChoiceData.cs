using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DialogueChoiceData
{
    [Header("Choice Text")]
    [SerializeField] private LocalizedString choiceText;

    [Header("Next Node")]
    [Tooltip("Индекс узла, в который перейдёт диалог после выбора этого ответа. -1 = завершить диалог.")]
    [SerializeField] private int nextNodeIndex = -1;

    public LocalizedString ChoiceText => choiceText;
    public int NextNodeIndex => nextNodeIndex;
}