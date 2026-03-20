using System;
using UnityEngine;

[Serializable]
public class DialogueActionData
{
    [SerializeField] private DialogueActionType actionType = DialogueActionType.None;

    [Header("Reward Action")]
    [SerializeField] private RewardData rewardData;

    [Header("Remove Item Action")]
    [SerializeField] private ItemData itemToRemove;
    [SerializeField] private int itemRemoveAmount = 1;

    [Header("Mark Played Action")]
    [Tooltip("Уникальный ключ. Например: villager_intro_completed")]
    [SerializeField] private string playedKey;

    [Header("Quest Action (Future)")]
    [SerializeField] private string questId;
    [SerializeField] private int questStateValue = 0;

    public DialogueActionType ActionType => actionType;
    public RewardData RewardData => rewardData;

    public ItemData ItemToRemove => itemToRemove;
    public int ItemRemoveAmount => itemRemoveAmount;

    public string PlayedKey => playedKey;

    public string QuestId => questId;
    public int QuestStateValue => questStateValue;
}