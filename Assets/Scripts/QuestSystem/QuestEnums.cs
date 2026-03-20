public enum QuestCategory
{
    MainStory = 0,
    SideQuest = 1
}

public enum QuestStatus
{
    NotStarted = 0,
    Active = 1,
    ReadyToTurnIn = 2,
    Completed = 3,
    Failed = 4
}

public enum QuestObjectiveType
{
    KillEnemy = 0,
    TalkToNpc = 1,
    EnterTriggerZone = 2,
    CollectItem = 3
}

public enum QuestStageCompletionMode
{
    CompleteAllObjectives = 0,
    CompleteAnyObjective = 1
}

public enum QuestItemObjectiveMode
{
    HaveInInventory = 0,
    DeliverOnTurnIn = 1
}