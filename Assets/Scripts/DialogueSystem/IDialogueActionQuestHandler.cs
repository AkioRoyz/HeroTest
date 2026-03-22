public interface IDialogueActionQuestHandler
{
    bool HandleQuestAction(string questId, QuestActionType actionType);
}