using System;
using System.Collections.Generic;
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

    [Header("Conditions")]
    [SerializeField] private List<DialogueConditionData> conditions = new();

    public LocalizedString ChoiceText => choiceText;
    public int NextNodeIndex => nextNodeIndex;
    public IReadOnlyList<DialogueConditionData> Conditions => conditions;
}