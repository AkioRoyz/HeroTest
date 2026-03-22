using UnityEngine;

public class QuestRuntimeDebugTester : MonoBehaviour
{
    [SerializeField] private QuestData testQuestData;

    private void Start()
    {
        if (testQuestData == null)
        {
            Debug.LogWarning("QuestRuntimeDebugTester: testQuestData is not assigned.");
            return;
        }

        QuestRuntimeData runtimeData = QuestRuntimeFactory.CreateFromData(testQuestData);

        if (runtimeData == null)
        {
            Debug.LogWarning("QuestRuntimeDebugTester: runtimeData creation failed.");
            return;
        }

        Debug.Log($"Quest runtime created: {runtimeData.QuestId}");
        Debug.Log($"Quest state: {runtimeData.QuestState}");
        Debug.Log($"Current step index: {runtimeData.CurrentStepIndex}");
        Debug.Log($"Steps count: {runtimeData.Steps.Count}");

        QuestStepRuntimeData currentStep = runtimeData.GetCurrentStep();
        if (currentStep != null)
        {
            Debug.Log($"Current step id: {currentStep.StepId}");
            Debug.Log($"Current step state: {currentStep.StepState}");
            Debug.Log($"Objectives count: {currentStep.Objectives.Count}");
        }
    }
}