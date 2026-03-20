using System;
using System.Collections.Generic;

[Serializable]
public class QuestRuntimeData
{
    private readonly Dictionary<string, int> objectiveProgress = new();

    public QuestData QuestData { get; private set; }
    public QuestStatus Status { get; private set; }
    public int CurrentStageIndex { get; private set; }
    public bool IsPinned { get; private set; }
    public string LastCompletedObjectiveId { get; private set; }

    public IReadOnlyDictionary<string, int> ObjectiveProgress => objectiveProgress;

    public QuestRuntimeData(QuestData questData)
    {
        QuestData = questData;
        Status = QuestStatus.Active;
        CurrentStageIndex = 0;
        IsPinned = false;
        LastCompletedObjectiveId = string.Empty;

        ResetCurrentStageProgress();
    }

    public void SetStatus(QuestStatus status)
    {
        Status = status;
    }

    public void SetPinned(bool isPinned)
    {
        IsPinned = isPinned;
    }

    public void SetStageIndex(int stageIndex)
    {
        CurrentStageIndex = stageIndex;
        ResetCurrentStageProgress();
        LastCompletedObjectiveId = string.Empty;
    }

    public void AdvanceStage()
    {
        CurrentStageIndex++;
        ResetCurrentStageProgress();
        LastCompletedObjectiveId = string.Empty;
    }

    public int GetProgress(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
            return 0;

        if (objectiveProgress.TryGetValue(objectiveId, out int value))
            return value;

        return 0;
    }

    public void SetProgress(string objectiveId, int value)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
            return;

        objectiveProgress[objectiveId] = value;
    }

    public void MarkObjectiveCompleted(string objectiveId)
    {
        LastCompletedObjectiveId = objectiveId;
    }

    public void ResetCurrentStageProgress()
    {
        objectiveProgress.Clear();

        if (QuestData == null)
            return;

        QuestStageData stage = QuestData.GetStage(CurrentStageIndex);
        if (stage == null)
            return;

        for (int i = 0; i < stage.Objectives.Count; i++)
        {
            QuestObjectiveData objective = stage.Objectives[i];
            if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
                continue;

            objectiveProgress[objective.ObjectiveId] = 0;
        }
    }
}