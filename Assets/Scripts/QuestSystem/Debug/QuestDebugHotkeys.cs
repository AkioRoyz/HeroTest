using System.Text;
using UnityEngine;

public class QuestDebugHotkeys : MonoBehaviour
{
    [Header("Quest To Inspect")]
    [SerializeField] private string testQuestId;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode printQuestKey = KeyCode.F1;
    [SerializeField] private KeyCode printAllActiveKey = KeyCode.F2;
    [SerializeField] private KeyCode acceptQuestKey = KeyCode.F3;
    [SerializeField] private KeyCode advanceQuestKey = KeyCode.F4;
    [SerializeField] private KeyCode readyToTurnInKey = KeyCode.F5;
    [SerializeField] private KeyCode completeQuestKey = KeyCode.F6;
    [SerializeField] private KeyCode failQuestKey = KeyCode.F7;

    private void Update()
    {
        if (Input.GetKeyDown(printQuestKey))
        {
            PrintSingleQuest();
        }

        if (Input.GetKeyDown(printAllActiveKey))
        {
            PrintAllActiveQuests();
        }

        if (Input.GetKeyDown(acceptQuestKey))
        {
            AcceptQuest();
        }

        if (Input.GetKeyDown(advanceQuestKey))
        {
            AdvanceQuest();
        }

        if (Input.GetKeyDown(readyToTurnInKey))
        {
            SetReadyToTurnIn();
        }

        if (Input.GetKeyDown(completeQuestKey))
        {
            CompleteQuest();
        }

        if (Input.GetKeyDown(failQuestKey))
        {
            FailQuest();
        }
    }

    private void PrintSingleQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        if (string.IsNullOrWhiteSpace(testQuestId))
        {
            Debug.LogWarning("QuestDebugHotkeys: testQuestId is empty.");
            return;
        }

        QuestRuntimeData runtime = QuestManager.Instance.GetQuestRuntime(testQuestId);
        QuestState state = QuestManager.Instance.GetQuestState(testQuestId);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== SINGLE QUEST DEBUG =====");
        sb.AppendLine($"Quest ID: {testQuestId}");
        sb.AppendLine($"State: {state}");

        if (runtime == null)
        {
            sb.AppendLine("Runtime: null");
            sb.AppendLine("===== END SINGLE QUEST DEBUG =====");
            Debug.Log(sb.ToString());
            return;
        }

        sb.AppendLine($"Current Step Index: {runtime.CurrentStepIndex}");
        sb.AppendLine($"Pinned: {runtime.IsPinned}");

        QuestStepRuntimeData currentStep = runtime.GetCurrentStep();
        if (currentStep != null)
        {
            sb.AppendLine($"Current Step ID: {currentStep.StepId}");
            sb.AppendLine($"Current Step State: {currentStep.StepState}");

            for (int i = 0; i < currentStep.Objectives.Count; i++)
            {
                QuestObjectiveRuntimeData objective = currentStep.Objectives[i];
                if (objective == null)
                    continue;

                sb.AppendLine(
                    $"Objective [{i}] " +
                    $"ID={objective.ObjectiveId}, " +
                    $"Type={objective.ObjectiveType}, " +
                    $"Target={objective.TargetId}, " +
                    $"Progress={objective.CurrentAmount}/{objective.RequiredAmount}, " +
                    $"Completed={objective.IsCompleted}, " +
                    $"Failed={objective.IsFailed}");
            }
        }

        sb.AppendLine("===== END SINGLE QUEST DEBUG =====");
        Debug.Log(sb.ToString());
    }

    private void PrintAllActiveQuests()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== ACTIVE QUESTS DEBUG =====");

        int count = 0;
        foreach (QuestRuntimeData runtime in QuestManager.Instance.ActiveQuests)
        {
            if (runtime == null)
                continue;

            count++;
            sb.AppendLine($"Quest ID: {runtime.QuestId}");
            sb.AppendLine($"State: {runtime.QuestState}");
            sb.AppendLine($"Current Step Index: {runtime.CurrentStepIndex}");

            QuestStepRuntimeData currentStep = runtime.GetCurrentStep();
            if (currentStep != null)
            {
                sb.AppendLine($"Current Step ID: {currentStep.StepId}");
                sb.AppendLine($"Current Step State: {currentStep.StepState}");
            }

            sb.AppendLine("----------------------------");
        }

        sb.AppendLine($"Total Active Quests: {count}");
        sb.AppendLine("===== END ACTIVE QUESTS DEBUG =====");
        Debug.Log(sb.ToString());
    }

    private void AcceptQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        bool result = QuestManager.Instance.AcceptQuest(testQuestId);
        Debug.Log($"AcceptQuest({testQuestId}) = {result}");
    }

    private void AdvanceQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        bool result = QuestManager.Instance.AdvanceQuestStep(testQuestId);
        Debug.Log($"AdvanceQuestStep({testQuestId}) = {result}");
    }

    private void SetReadyToTurnIn()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        bool result = QuestManager.Instance.SetQuestReadyToTurnIn(testQuestId);
        Debug.Log($"SetQuestReadyToTurnIn({testQuestId}) = {result}");
    }

    private void CompleteQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        bool result = QuestManager.Instance.CompleteQuest(testQuestId);
        Debug.Log($"CompleteQuest({testQuestId}) = {result}");
    }

    private void FailQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestDebugHotkeys: QuestManager.Instance is missing.");
            return;
        }

        bool result = QuestManager.Instance.FailQuest(testQuestId);
        Debug.Log($"FailQuest({testQuestId}) = {result}");
    }
}