using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : MonoBehaviour, IDialogueQuestProvider, IDialogueActionQuestHandler
{
    public static QuestSystem Instance;

    [Header("Quest Database")]
    [SerializeField] private List<QuestData> questDatabase = new();

    [Header("Pinned")]
    [SerializeField] private int maxPinnedQuests = 5;

    private readonly Dictionary<string, QuestData> questLookup = new();
    private readonly Dictionary<string, QuestRuntimeData> runtimeQuests = new();
    private readonly List<string> pinnedQuestIds = new();
    private readonly HashSet<string> permanentlyCompletedQuestIds = new();

    public event Action<QuestData> OnQuestAccepted;
    public event Action<QuestData> OnQuestUpdated;
    public event Action<QuestData> OnQuestReadyToTurnIn;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestFailed;
    public event Action OnPinnedQuestsChanged;

    public IReadOnlyList<string> PinnedQuestIds => pinnedQuestIds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

    private void OnEnable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void BuildLookup()
    {
        questLookup.Clear();

        for (int i = 0; i < questDatabase.Count; i++)
        {
            QuestData quest = questDatabase[i];
            if (quest == null)
                continue;

            if (string.IsNullOrWhiteSpace(quest.QuestId))
            {
                Debug.LogWarning($"Quest '{quest.name}' has empty QuestId and was skipped.");
                continue;
            }

            if (questLookup.ContainsKey(quest.QuestId))
            {
                Debug.LogWarning($"Duplicate QuestId found: {quest.QuestId}");
                continue;
            }

            questLookup.Add(quest.QuestId, quest);
        }
    }

    private void HandleInventoryChanged()
    {
        RefreshAllItemObjectives();
    }

    public bool HasQuest(string questId)
    {
        return runtimeQuests.ContainsKey(questId);
    }

    public QuestData GetQuestData(string questId)
    {
        questLookup.TryGetValue(questId, out QuestData questData);
        return questData;
    }

    public QuestRuntimeData GetRuntimeQuest(string questId)
    {
        runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData);
        return runtimeData;
    }

    public QuestStatus GetQuestStatus(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return QuestStatus.NotStarted;

        return runtimeData.Status;
    }

    public bool CanAcceptQuest(string questId)
    {
        QuestData questData = GetQuestData(questId);
        if (questData == null)
            return false;

        if (runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
        {
            if (runtimeData.Status == QuestStatus.Active || runtimeData.Status == QuestStatus.ReadyToTurnIn)
                return false;

            if (runtimeData.Status == QuestStatus.Completed && !questData.Repeatable)
                return false;

            if (runtimeData.Status == QuestStatus.Completed && questData.Repeatable)
                return true;

            if (runtimeData.Status == QuestStatus.Failed)
                return true;
        }

        if (permanentlyCompletedQuestIds.Contains(questId) && !questData.Repeatable)
            return false;

        return true;
    }

    public bool AcceptQuest(string questId)
    {
        if (!CanAcceptQuest(questId))
            return false;

        QuestData questData = GetQuestData(questId);
        if (questData == null)
            return false;

        QuestRuntimeData runtimeData = new QuestRuntimeData(questData);
        runtimeQuests[questId] = runtimeData;

        RefreshItemObjectives(runtimeData);
        EvaluateStageCompletion(runtimeData);

        OnQuestAccepted?.Invoke(questData);
        OnQuestUpdated?.Invoke(questData);
        return true;
    }

    public bool AdvanceQuestStage(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return false;

        if (runtimeData.Status != QuestStatus.Active)
            return false;

        QuestData questData = runtimeData.QuestData;
        if (questData == null)
            return false;

        int nextStageIndex = runtimeData.CurrentStageIndex + 1;

        if (nextStageIndex >= questData.Stages.Count)
        {
            if (questData.RequiresTurnIn)
            {
                runtimeData.SetStatus(QuestStatus.ReadyToTurnIn);
                OnQuestReadyToTurnIn?.Invoke(questData);
            }
            else
            {
                CompleteQuestInternal(runtimeData);
            }

            OnQuestUpdated?.Invoke(questData);
            return true;
        }

        runtimeData.SetStageIndex(nextStageIndex);
        RefreshItemObjectives(runtimeData);
        EvaluateStageCompletion(runtimeData);

        OnQuestUpdated?.Invoke(questData);
        return true;
    }

    public bool SetQuestStage(string questId, int stageIndex)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return false;

        QuestData questData = runtimeData.QuestData;
        if (questData == null)
            return false;

        if (stageIndex < 0 || stageIndex >= questData.Stages.Count)
            return false;

        runtimeData.SetStatus(QuestStatus.Active);
        runtimeData.SetStageIndex(stageIndex);

        RefreshItemObjectives(runtimeData);
        EvaluateStageCompletion(runtimeData);

        OnQuestUpdated?.Invoke(questData);
        return true;
    }

    public bool TurnInQuest(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return false;

        if (runtimeData.Status != QuestStatus.ReadyToTurnIn)
            return false;

        QuestData questData = runtimeData.QuestData;
        if (questData == null)
            return false;

        if (!TryConsumeDeliverItems(runtimeData))
            return false;

        if (RewardSystem.Instance != null)
        {
            RewardSystem.Instance.GiveReward(questData.RewardData);
        }

        CompleteQuestInternal(runtimeData);
        return true;
    }

    public bool FailQuest(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return false;

        QuestData questData = runtimeData.QuestData;
        if (questData == null)
            return false;

        runtimeData.SetStatus(QuestStatus.Failed);
        runtimeData.SetPinned(false);
        pinnedQuestIds.Remove(questId);

        OnQuestFailed?.Invoke(questData);
        OnQuestUpdated?.Invoke(questData);
        OnPinnedQuestsChanged?.Invoke();
        return true;
    }

    public bool RestartQuest(string questId)
    {
        QuestData questData = GetQuestData(questId);
        if (questData == null)
            return false;

        if (runtimeQuests.TryGetValue(questId, out QuestRuntimeData oldRuntime))
        {
            if (oldRuntime.Status != QuestStatus.Failed && oldRuntime.Status != QuestStatus.Completed)
                return false;
        }

        QuestRuntimeData newRuntime = new QuestRuntimeData(questData);
        runtimeQuests[questId] = newRuntime;

        RefreshItemObjectives(newRuntime);
        EvaluateStageCompletion(newRuntime);

        OnQuestAccepted?.Invoke(questData);
        OnQuestUpdated?.Invoke(questData);
        return true;
    }

    public bool PinQuest(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return false;

        if (runtimeData.Status != QuestStatus.Active && runtimeData.Status != QuestStatus.ReadyToTurnIn)
            return false;

        if (runtimeData.IsPinned)
            return true;

        if (pinnedQuestIds.Count >= maxPinnedQuests)
            return false;

        runtimeData.SetPinned(true);
        pinnedQuestIds.Add(questId);

        OnPinnedQuestsChanged?.Invoke();
        OnQuestUpdated?.Invoke(runtimeData.QuestData);
        return true;
    }

    public bool UnpinQuest(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return false;

        if (!runtimeData.IsPinned)
            return true;

        runtimeData.SetPinned(false);
        pinnedQuestIds.Remove(questId);

        OnPinnedQuestsChanged?.Invoke();
        OnQuestUpdated?.Invoke(runtimeData.QuestData);
        return true;
    }

    public List<QuestRuntimeData> GetActiveQuests()
    {
        List<QuestRuntimeData> result = new List<QuestRuntimeData>();

        foreach (var pair in runtimeQuests)
        {
            if (pair.Value.Status == QuestStatus.Active || pair.Value.Status == QuestStatus.ReadyToTurnIn)
            {
                result.Add(pair.Value);
            }
        }

        return result;
    }

    public List<QuestRuntimeData> GetCompletedQuests()
    {
        List<QuestRuntimeData> result = new List<QuestRuntimeData>();

        foreach (var pair in runtimeQuests)
        {
            if (pair.Value.Status == QuestStatus.Completed)
            {
                result.Add(pair.Value);
            }
        }

        return result;
    }

    public List<QuestRuntimeData> GetFailedQuests()
    {
        List<QuestRuntimeData> result = new List<QuestRuntimeData>();

        foreach (var pair in runtimeQuests)
        {
            if (pair.Value.Status == QuestStatus.Failed)
            {
                result.Add(pair.Value);
            }
        }

        return result;
    }

    public QuestStageData GetCurrentStage(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return null;

        return runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
    }

    public QuestObjectiveData GetFirstIncompleteObjective(string questId)
    {
        if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
            return null;

        QuestStageData stage = runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
        if (stage == null)
            return null;

        for (int i = 0; i < stage.Objectives.Count; i++)
        {
            QuestObjectiveData objective = stage.Objectives[i];
            if (objective == null)
                continue;

            if (!IsObjectiveCompleted(runtimeData, objective))
                return objective;
        }

        return null;
    }

    public void RegisterEnemyKilled(EnemyType enemyType)
    {
        List<QuestRuntimeData> activeQuests = GetActiveQuests();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestRuntimeData runtimeData = activeQuests[i];

            if (runtimeData.Status != QuestStatus.Active)
                continue;

            QuestStageData stage = runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
            if (stage == null)
                continue;

            bool changed = false;

            for (int j = 0; j < stage.Objectives.Count; j++)
            {
                QuestObjectiveData objective = stage.Objectives[j];
                if (objective == null)
                    continue;

                if (objective.ObjectiveType != QuestObjectiveType.KillEnemy)
                    continue;

                if (objective.EnemyType != EnemyType.Any && objective.EnemyType != enemyType)
                    continue;

                int current = runtimeData.GetProgress(objective.ObjectiveId);
                int next = Mathf.Min(current + 1, objective.RequiredAmount);

                if (next != current)
                {
                    runtimeData.SetProgress(objective.ObjectiveId, next);

                    if (next >= objective.RequiredAmount)
                    {
                        runtimeData.MarkObjectiveCompleted(objective.ObjectiveId);
                    }

                    changed = true;
                }
            }

            if (changed)
            {
                EvaluateStageCompletion(runtimeData);
                OnQuestUpdated?.Invoke(runtimeData.QuestData);
            }
        }
    }

    public void RegisterNpcTalked(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return;

        List<QuestRuntimeData> activeQuests = GetActiveQuests();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestRuntimeData runtimeData = activeQuests[i];

            if (runtimeData.Status != QuestStatus.Active)
                continue;

            QuestStageData stage = runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
            if (stage == null)
                continue;

            bool changed = false;

            for (int j = 0; j < stage.Objectives.Count; j++)
            {
                QuestObjectiveData objective = stage.Objectives[j];
                if (objective == null)
                    continue;

                if (objective.ObjectiveType != QuestObjectiveType.TalkToNpc)
                    continue;

                if (!string.Equals(objective.NpcId, npcId, StringComparison.Ordinal))
                    continue;

                int current = runtimeData.GetProgress(objective.ObjectiveId);
                int next = Mathf.Min(current + 1, objective.RequiredAmount);

                if (next != current)
                {
                    runtimeData.SetProgress(objective.ObjectiveId, next);

                    if (next >= objective.RequiredAmount)
                    {
                        runtimeData.MarkObjectiveCompleted(objective.ObjectiveId);
                    }

                    changed = true;
                }
            }

            if (changed)
            {
                EvaluateStageCompletion(runtimeData);
                OnQuestUpdated?.Invoke(runtimeData.QuestData);
            }
        }
    }

    public void RegisterTriggerEntered(string triggerId)
    {
        if (string.IsNullOrWhiteSpace(triggerId))
            return;

        List<QuestRuntimeData> activeQuests = GetActiveQuests();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestRuntimeData runtimeData = activeQuests[i];

            if (runtimeData.Status != QuestStatus.Active)
                continue;

            QuestStageData stage = runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
            if (stage == null)
                continue;

            bool changed = false;

            for (int j = 0; j < stage.Objectives.Count; j++)
            {
                QuestObjectiveData objective = stage.Objectives[j];
                if (objective == null)
                    continue;

                if (objective.ObjectiveType != QuestObjectiveType.EnterTriggerZone)
                    continue;

                if (!string.Equals(objective.TriggerId, triggerId, StringComparison.Ordinal))
                    continue;

                int current = runtimeData.GetProgress(objective.ObjectiveId);
                int next = Mathf.Min(current + 1, objective.RequiredAmount);

                if (next != current)
                {
                    runtimeData.SetProgress(objective.ObjectiveId, next);

                    if (next >= objective.RequiredAmount)
                    {
                        runtimeData.MarkObjectiveCompleted(objective.ObjectiveId);
                    }

                    changed = true;
                }
            }

            if (changed)
            {
                EvaluateStageCompletion(runtimeData);
                OnQuestUpdated?.Invoke(runtimeData.QuestData);
            }
        }
    }

    public void RefreshAllItemObjectives()
    {
        List<QuestRuntimeData> activeQuests = GetActiveQuests();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            RefreshItemObjectives(activeQuests[i]);
            EvaluateStageCompletion(activeQuests[i]);
            OnQuestUpdated?.Invoke(activeQuests[i].QuestData);
        }
    }

    private void RefreshItemObjectives(QuestRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.Status != QuestStatus.Active)
            return;

        QuestStageData stage = runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
        if (stage == null)
            return;

        for (int i = 0; i < stage.Objectives.Count; i++)
        {
            QuestObjectiveData objective = stage.Objectives[i];
            if (objective == null)
                continue;

            if (objective.ObjectiveType != QuestObjectiveType.CollectItem)
                continue;

            if (objective.RequiredItem == null || InventorySystem.Instance == null)
                continue;

            int itemCount = InventorySystem.Instance.GetItemCount(objective.RequiredItem);
            int progress = Mathf.Min(itemCount, objective.RequiredAmount);

            runtimeData.SetProgress(objective.ObjectiveId, progress);

            if (progress >= objective.RequiredAmount)
            {
                runtimeData.MarkObjectiveCompleted(objective.ObjectiveId);
            }
        }
    }

    private void EvaluateStageCompletion(QuestRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.Status != QuestStatus.Active)
            return;

        QuestStageData stage = runtimeData.QuestData.GetStage(runtimeData.CurrentStageIndex);
        if (stage == null)
            return;

        bool stageCompleted = false;

        if (stage.CompletionMode == QuestStageCompletionMode.CompleteAllObjectives)
        {
            stageCompleted = true;

            for (int i = 0; i < stage.Objectives.Count; i++)
            {
                QuestObjectiveData objective = stage.Objectives[i];
                if (objective == null)
                    continue;

                if (!IsObjectiveCompleted(runtimeData, objective))
                {
                    stageCompleted = false;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < stage.Objectives.Count; i++)
            {
                QuestObjectiveData objective = stage.Objectives[i];
                if (objective == null)
                    continue;

                if (IsObjectiveCompleted(runtimeData, objective))
                {
                    stageCompleted = true;
                    break;
                }
            }
        }

        if (!stageCompleted)
            return;

        bool isLastStage = runtimeData.CurrentStageIndex >= runtimeData.QuestData.Stages.Count - 1;

        if (!isLastStage)
        {
            runtimeData.AdvanceStage();
            RefreshItemObjectives(runtimeData);
            return;
        }

        if (runtimeData.QuestData.RequiresTurnIn)
        {
            runtimeData.SetStatus(QuestStatus.ReadyToTurnIn);
            OnQuestReadyToTurnIn?.Invoke(runtimeData.QuestData);
        }
        else
        {
            CompleteQuestInternal(runtimeData);
        }
    }

    private bool IsObjectiveCompleted(QuestRuntimeData runtimeData, QuestObjectiveData objective)
    {
        int progress = runtimeData.GetProgress(objective.ObjectiveId);
        return progress >= objective.RequiredAmount;
    }

    private bool TryConsumeDeliverItems(QuestRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.QuestData == null)
            return false;

        for (int stageIndex = 0; stageIndex < runtimeData.QuestData.Stages.Count; stageIndex++)
        {
            QuestStageData stage = runtimeData.QuestData.GetStage(stageIndex);
            if (stage == null)
                continue;

            for (int i = 0; i < stage.Objectives.Count; i++)
            {
                QuestObjectiveData objective = stage.Objectives[i];
                if (objective == null)
                    continue;

                if (objective.ObjectiveType != QuestObjectiveType.CollectItem)
                    continue;

                if (objective.ItemObjectiveMode != QuestItemObjectiveMode.DeliverOnTurnIn)
                    continue;

                if (objective.RequiredItem == null || InventorySystem.Instance == null)
                    return false;

                if (!InventorySystem.Instance.HasItem(objective.RequiredItem, objective.RequiredAmount))
                    return false;
            }
        }

        for (int stageIndex = 0; stageIndex < runtimeData.QuestData.Stages.Count; stageIndex++)
        {
            QuestStageData stage = runtimeData.QuestData.GetStage(stageIndex);
            if (stage == null)
                continue;

            for (int i = 0; i < stage.Objectives.Count; i++)
            {
                QuestObjectiveData objective = stage.Objectives[i];
                if (objective == null)
                    continue;

                if (objective.ObjectiveType != QuestObjectiveType.CollectItem)
                    continue;

                if (objective.ItemObjectiveMode != QuestItemObjectiveMode.DeliverOnTurnIn)
                    continue;

                InventorySystem.Instance.RemoveItem(objective.RequiredItem, objective.RequiredAmount);
            }
        }

        return true;
    }

    private void CompleteQuestInternal(QuestRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.QuestData == null)
            return;

        QuestData questData = runtimeData.QuestData;

        runtimeData.SetStatus(QuestStatus.Completed);
        runtimeData.SetPinned(false);
        pinnedQuestIds.Remove(questData.QuestId);

        if (!questData.Repeatable)
        {
            permanentlyCompletedQuestIds.Add(questData.QuestId);
        }

        OnQuestCompleted?.Invoke(questData);
        OnQuestUpdated?.Invoke(questData);
        OnPinnedQuestsChanged?.Invoke();
    }

    // ---------------- DIALOGUE BRIDGE ----------------

    public bool IsQuestStateMatched(string questId, int requiredStateValue)
    {
        QuestStatus currentStatus = GetQuestStatus(questId);

        if (DialogueQuestStateValues.IsStageValue(requiredStateValue))
        {
            if (!runtimeQuests.TryGetValue(questId, out QuestRuntimeData runtimeData))
                return false;

            if (runtimeData.Status != QuestStatus.Active && runtimeData.Status != QuestStatus.ReadyToTurnIn)
                return false;

            int requiredStage = DialogueQuestStateValues.ExtractStageIndex(requiredStateValue);
            return runtimeData.CurrentStageIndex == requiredStage;
        }

        switch (requiredStateValue)
        {
            case DialogueQuestStateValues.NotStarted:
                return currentStatus == QuestStatus.NotStarted;

            case DialogueQuestStateValues.Active:
                return currentStatus == QuestStatus.Active;

            case DialogueQuestStateValues.ReadyToTurnIn:
                return currentStatus == QuestStatus.ReadyToTurnIn;

            case DialogueQuestStateValues.Completed:
                return currentStatus == QuestStatus.Completed;

            case DialogueQuestStateValues.Failed:
                return currentStatus == QuestStatus.Failed;

            default:
                return false;
        }
    }

    public void ExecuteQuestAction(string questId, int questStateValue)
    {
        if (string.IsNullOrWhiteSpace(questId))
            return;

        if (DialogueQuestStateValues.IsStageValue(questStateValue))
        {
            int stageIndex = DialogueQuestStateValues.ExtractStageIndex(questStateValue);
            SetQuestStage(questId, stageIndex);
            return;
        }

        switch (questStateValue)
        {
            case DialogueQuestStateValues.AcceptQuest:
                AcceptQuest(questId);
                break;

            case DialogueQuestStateValues.TurnInQuest:
                TurnInQuest(questId);
                break;

            case DialogueQuestStateValues.FailQuest:
                FailQuest(questId);
                break;

            case DialogueQuestStateValues.RestartQuest:
                RestartQuest(questId);
                break;

            case DialogueQuestStateValues.AdvanceStage:
                AdvanceQuestStage(questId);
                break;
        }
    }
}