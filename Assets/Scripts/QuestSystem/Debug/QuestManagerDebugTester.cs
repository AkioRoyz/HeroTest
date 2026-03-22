using System.Collections;
using UnityEngine;

public class QuestManagerDebugTester : MonoBehaviour
{
    [SerializeField] private string testQuestId;
    [SerializeField] private bool runTestOnStart = true;

    private IEnumerator Start()
    {
        if (!runTestOnStart)
            yield break;

        yield return null;

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestManagerDebugTester: QuestManager.Instance is missing.");
            yield break;
        }

        Debug.Log("=== QUEST MANAGER DEBUG TEST START ===");

        bool accepted = QuestManager.Instance.AcceptQuest(testQuestId);
        Debug.Log($"AcceptQuest({testQuestId}) = {accepted}");
        Debug.Log($"State after accept = {QuestManager.Instance.GetQuestState(testQuestId)}");

        bool pinned = QuestManager.Instance.PinQuest(testQuestId);
        Debug.Log($"PinQuest({testQuestId}) = {pinned}");
        Debug.Log($"Pinned count = {QuestManager.Instance.GetPinnedQuestCount()}");

        bool advanced = QuestManager.Instance.AdvanceQuestStep(testQuestId);
        Debug.Log($"AdvanceQuestStep({testQuestId}) = {advanced}");
        Debug.Log($"State after advance = {QuestManager.Instance.GetQuestState(testQuestId)}");

        QuestRuntimeData runtime = QuestManager.Instance.GetQuestRuntime(testQuestId);
        if (runtime != null)
        {
            Debug.Log($"Current step index = {runtime.CurrentStepIndex}");

            QuestStepRuntimeData step = runtime.GetCurrentStep();
            if (step != null)
            {
                Debug.Log($"Current step id = {step.StepId}");
                Debug.Log($"Current step state = {step.StepState}");
            }
        }

        bool readyToTurnIn = QuestManager.Instance.SetQuestReadyToTurnIn(testQuestId);
        Debug.Log($"SetQuestReadyToTurnIn({testQuestId}) = {readyToTurnIn}");
        Debug.Log($"State after ready = {QuestManager.Instance.GetQuestState(testQuestId)}");

        bool completed = QuestManager.Instance.CompleteQuest(testQuestId);
        Debug.Log($"CompleteQuest({testQuestId}) = {completed}");
        Debug.Log($"State after complete = {QuestManager.Instance.GetQuestState(testQuestId)}");

        Debug.Log("=== QUEST MANAGER DEBUG TEST END ===");
    }
}