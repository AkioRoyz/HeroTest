using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Quest_", menuName = "Game/Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Base")]
    [SerializeField] private string questId;
    [SerializeField] private QuestCategory category = QuestCategory.SideQuest;
    [SerializeField] private bool repeatable = false;

    [Header("UI")]
    [SerializeField] private LocalizedString title;
    [SerializeField] private LocalizedString shortDescription;
    [SerializeField] private LocalizedString fullDescription;

    [Header("Quest Source Visibility")]
    [Tooltip("Если false, квест не должен показываться иконкой над NPC / trigger источником.")]
    [SerializeField] private bool showQuestIconAtSource = true;

    [Header("Turn In")]
    [Tooltip("Если true, после выполнения этапов квест перейдёт в ReadyToTurnIn и будет ждать сдачи.")]
    [SerializeField] private bool requiresTurnIn = true;

    [Header("Reward")]
    [SerializeField] private RewardData rewardData;

    [Header("Stages")]
    [SerializeField] private List<QuestStageData> stages = new();

    public string QuestId => questId;
    public QuestCategory Category => category;
    public bool Repeatable => repeatable;

    public LocalizedString Title => title;
    public LocalizedString ShortDescription => shortDescription;
    public LocalizedString FullDescription => fullDescription;

    public bool ShowQuestIconAtSource => showQuestIconAtSource;
    public bool RequiresTurnIn => requiresTurnIn;

    public RewardData RewardData => rewardData;
    public IReadOnlyList<QuestStageData> Stages => stages;

    public QuestStageData GetStage(int index)
    {
        if (index < 0 || index >= stages.Count)
            return null;

        return stages[index];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateData();
    }
#endif

    public void ValidateData()
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            Debug.LogWarning($"Quest '{name}' has empty QuestId.");
        }

        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning($"Quest '{name}' has no stages.");
            return;
        }

        for (int i = 0; i < stages.Count; i++)
        {
            QuestStageData stage = stages[i];
            if (stage == null)
                continue;

            stage.Validate(name, i);
        }
    }
}

[Serializable]
public class QuestStageData
{
    [Header("Stage UI")]
    [SerializeField] private LocalizedString stageName;
    [SerializeField] private LocalizedString stageDescription;

    [Header("Completion")]
    [SerializeField] private QuestStageCompletionMode completionMode = QuestStageCompletionMode.CompleteAllObjectives;

    [Header("Objectives")]
    [SerializeField] private List<QuestObjectiveData> objectives = new();

    public LocalizedString StageName => stageName;
    public LocalizedString StageDescription => stageDescription;
    public QuestStageCompletionMode CompletionMode => completionMode;
    public IReadOnlyList<QuestObjectiveData> Objectives => objectives;

    public void Validate(string questName, int stageIndex)
    {
        if (objectives == null || objectives.Count == 0)
        {
            Debug.LogWarning($"Quest '{questName}' stage [{stageIndex}] has no objectives.");
            return;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjectiveData objective = objectives[i];
            if (objective == null)
                continue;

            objective.Validate(questName, stageIndex, i);
        }
    }
}

[Serializable]
public class QuestObjectiveData
{
    [Header("Base")]
    [SerializeField] private string objectiveId;
    [SerializeField] private QuestObjectiveType objectiveType;
    [SerializeField] private LocalizedString description;

    [Header("Progress")]
    [SerializeField] private int requiredAmount = 1;

    [Header("Enemy Objective")]
    [SerializeField] private EnemyType enemyType = EnemyType.Any;

    [Header("NPC Objective")]
    [Tooltip("Например: elder_npc, guard_01")]
    [SerializeField] private string npcId;

    [Header("Trigger Objective")]
    [Tooltip("Например: cave_entrance, forest_ruins")]
    [SerializeField] private string triggerId;

    [Header("Item Objective")]
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private QuestItemObjectiveMode itemObjectiveMode = QuestItemObjectiveMode.HaveInInventory;

    public string ObjectiveId => objectiveId;
    public QuestObjectiveType ObjectiveType => objectiveType;
    public LocalizedString Description => description;
    public int RequiredAmount => requiredAmount;

    public EnemyType EnemyType => enemyType;
    public string NpcId => npcId;
    public string TriggerId => triggerId;

    public ItemData RequiredItem => requiredItem;
    public QuestItemObjectiveMode ItemObjectiveMode => itemObjectiveMode;

    public void Validate(string questName, int stageIndex, int objectiveIndex)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
        {
            Debug.LogWarning($"Quest '{questName}' stage [{stageIndex}] objective [{objectiveIndex}] has empty ObjectiveId.");
        }

        if (requiredAmount <= 0)
        {
            Debug.LogWarning($"Quest '{questName}' stage [{stageIndex}] objective [{objectiveIndex}] has invalid RequiredAmount.");
        }

        if (objectiveType == QuestObjectiveType.TalkToNpc && string.IsNullOrWhiteSpace(npcId))
        {
            Debug.LogWarning($"Quest '{questName}' stage [{stageIndex}] objective [{objectiveIndex}] TalkToNpc has empty npcId.");
        }

        if (objectiveType == QuestObjectiveType.EnterTriggerZone && string.IsNullOrWhiteSpace(triggerId))
        {
            Debug.LogWarning($"Quest '{questName}' stage [{stageIndex}] objective [{objectiveIndex}] EnterTriggerZone has empty triggerId.");
        }

        if (objectiveType == QuestObjectiveType.CollectItem && requiredItem == null)
        {
            Debug.LogWarning($"Quest '{questName}' stage [{stageIndex}] objective [{objectiveIndex}] CollectItem has null requiredItem.");
        }
    }
}