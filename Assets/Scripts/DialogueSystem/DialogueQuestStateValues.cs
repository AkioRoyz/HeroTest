public static class DialogueQuestStateValues
{
    // ”слови€ / состо€ни€ дл€ диалогов
    public const int NotStarted = 0;
    public const int Active = 1;
    public const int ReadyToTurnIn = 2;
    public const int Completed = 3;
    public const int Failed = 4;

    // ƒействи€ дл€ диалогов
    public const int AcceptQuest = 100;
    public const int TurnInQuest = 101;
    public const int FailQuest = 102;
    public const int RestartQuest = 103;
    public const int AdvanceStage = 104;

    // ѕроверка конкретного этапа:
    // 1000 + индекс этапа
    // пример: 1000 = этап 0, 1001 = этап 1, 1002 = этап 2
    public const int StageBase = 1000;

    public static int Stage(int stageIndex)
    {
        return StageBase + stageIndex;
    }

    public static bool IsStageValue(int value)
    {
        return value >= StageBase;
    }

    public static int ExtractStageIndex(int value)
    {
        return value - StageBase;
    }
}