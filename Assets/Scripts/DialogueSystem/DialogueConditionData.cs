using System;
using UnityEngine;

[Serializable]
public class DialogueConditionData
{
    [SerializeField] private DialogueConditionType conditionType = DialogueConditionType.None;

    [Header("Level Condition")]
    [SerializeField] private int requiredLevel = 1;

    [Header("Item Condition")]
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private int requiredItemAmount = 1;

    [Header("Play Once Condition")]
    [Tooltip("Уникальный ключ. Например: villager_intro_line_01")]
    [SerializeField] private string onceKey;

    [Header("Quest Condition")]
    [SerializeField] private string questId;
    [SerializeField] private int requiredQuestState = 0;

    [Header("Quest Step Condition")]
    [SerializeField] private int requiredQuestStepIndex = 0;

    public DialogueConditionType ConditionType => conditionType;
    public int RequiredLevel => requiredLevel;
    public ItemData RequiredItem => requiredItem;
    public int RequiredItemAmount => requiredItemAmount;
    public string OnceKey => onceKey;
    public string QuestId => questId;
    public int RequiredQuestState => requiredQuestState;
    public int RequiredQuestStepIndex => requiredQuestStepIndex;
}