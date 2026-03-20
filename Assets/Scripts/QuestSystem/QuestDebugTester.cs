using UnityEngine;

public class QuestDebugTester : MonoBehaviour
{
    [Header("Test Quest")]
    [SerializeField] private string questId = "kill_first_enemy";

    [Header("Test IDs")]
    [SerializeField] private string testNpcId = "elder_npc";
    [SerializeField] private string testTriggerId = "test_zone";

    [Header("Test Enemy Type")]
    [SerializeField] private EnemyType testEnemyType = EnemyType.Any;

    private void Update()
    {
        // 1 - взять квест
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (QuestSystem.Instance != null)
            {
                bool result = QuestSystem.Instance.AcceptQuest(questId);
                Debug.Log($"[QuestDebugTester] AcceptQuest({questId}) = {result}");
                PrintQuestState();
            }
        }

        // 2 - симулировать убийство врага
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.RegisterEnemyKilled(testEnemyType);
                Debug.Log($"[QuestDebugTester] RegisterEnemyKilled({testEnemyType})");
                PrintQuestState();
            }
        }

        // 3 - симулировать разговор с NPC
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.RegisterNpcTalked(testNpcId);
                Debug.Log($"[QuestDebugTester] RegisterNpcTalked({testNpcId})");
                PrintQuestState();
            }
        }

        // 4 - симулировать вход в триггер
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.RegisterTriggerEntered(testTriggerId);
                Debug.Log($"[QuestDebugTester] RegisterTriggerEntered({testTriggerId})");
                PrintQuestState();
            }
        }

        // 5 - перейти на следующий этап вручную
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (QuestSystem.Instance != null)
            {
                bool result = QuestSystem.Instance.AdvanceQuestStage(questId);
                Debug.Log($"[QuestDebugTester] AdvanceQuestStage({questId}) = {result}");
                PrintQuestState();
            }
        }

        // 6 - сдать квест
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (QuestSystem.Instance != null)
            {
                bool result = QuestSystem.Instance.TurnInQuest(questId);
                Debug.Log($"[QuestDebugTester] TurnInQuest({questId}) = {result}");
                PrintQuestState();
            }
        }

        // 7 - провалить квест
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (QuestSystem.Instance != null)
            {
                bool result = QuestSystem.Instance.FailQuest(questId);
                Debug.Log($"[QuestDebugTester] FailQuest({questId}) = {result}");
                PrintQuestState();
            }
        }

        // 8 - перезапустить квест
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (QuestSystem.Instance != null)
            {
                bool result = QuestSystem.Instance.RestartQuest(questId);
                Debug.Log($"[QuestDebugTester] RestartQuest({questId}) = {result}");
                PrintQuestState();
            }
        }

        // 9 - вывести состояние в лог
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PrintQuestState();
        }
    }

    private void PrintQuestState()
    {
        if (QuestSystem.Instance == null)
        {
            Debug.LogWarning("[QuestDebugTester] QuestSystem.Instance is missing.");
            return;
        }

        QuestRuntimeData runtime = QuestSystem.Instance.GetRuntimeQuest(questId);

        if (runtime == null)
        {
            Debug.Log($"[QuestDebugTester] Quest '{questId}' is not started.");
            return;
        }

        Debug.Log(
            $"[QuestDebugTester] Quest '{questId}' | Status = {runtime.Status} | Stage = {runtime.CurrentStageIndex}");
    }
}