using System.Text;
using UnityEngine;

public class QuestDebugStatusPrinter : MonoBehaviour
{
    [SerializeField] private string questId;

    [ContextMenu("Print Quest Status")]
    public void PrintQuestStatus()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugStatusPrinter: QuestManager.Instance is missing.");
            return;
        }

        if (string.IsNullOrWhiteSpace(questId))
        {
            Debug.LogWarning("QuestDebugStatusPrinter: questId is empty.");
            return;
        }

        QuestData questData = QuestManager.Instance.GetQuestData(questId);
        QuestRuntimeData runtime = QuestManager.Instance.GetQuestRuntime(questId);
        QuestState questState = QuestManager.Instance.GetQuestState(questId);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("===== QUEST DEBUG STATUS =====");
        sb.AppendLine($"Quest ID: {questId}");
        sb.AppendLine($"Quest Data Found: {questData != null}");
        sb.AppendLine($"Quest Runtime Found: {runtime != null}");
        sb.AppendLine($"Quest State: {questState}");

        if (runtime == null)
        {
            sb.AppendLine("Runtime is null. Quest is probably not started yet.");
            sb.AppendLine("===== END QUEST DEBUG STATUS =====");
            Debug.Log(sb.ToString());
            return;
        }

        sb.AppendLine($"Current Step Index: {runtime.CurrentStepIndex}");
        sb.AppendLine($"Is Pinned: {runtime.IsPinned}");
        sb.AppendLine($"Total Runtime Steps: {runtime.Steps.Count}");

        QuestStepRuntimeData currentStep = runtime.GetCurrentStep();
        if (currentStep != null)
        {
            sb.AppendLine($"Current Step ID: {currentStep.StepId}");
            sb.AppendLine($"Current Step State: {currentStep.StepState}");
        }
        else
        {
            sb.AppendLine("Current Step: null");
        }

        sb.AppendLine("----- Steps -----");

        for (int i = 0; i < runtime.Steps.Count; i++)
        {
            QuestStepRuntimeData step = runtime.Steps[i];
            if (step == null)
                continue;

            sb.AppendLine($"Step [{i}] ID: {step.StepId}");
            sb.AppendLine($"Step [{i}] State: {step.StepState}");
            sb.AppendLine($"Step [{i}] Objectives Count: {step.Objectives.Count}");

            for (int j = 0; j < step.Objectives.Count; j++)
            {
                QuestObjectiveRuntimeData objective = step.Objectives[j];
                if (objective == null)
                    continue;

                sb.AppendLine(
                    $"  Objective [{j}] -> " +
                    $"ID: {objective.ObjectiveId}, " +
                    $"Type: {objective.ObjectiveType}, " +
                    $"TargetId: {objective.TargetId}, " +
                    $"Progress: {objective.CurrentAmount}/{objective.RequiredAmount}, " +
                    $"Completed: {objective.IsCompleted}, " +
                    $"Failed: {objective.IsFailed}");
            }
        }

        sb.AppendLine("===== END QUEST DEBUG STATUS =====");
        Debug.Log(sb.ToString());
    }
}