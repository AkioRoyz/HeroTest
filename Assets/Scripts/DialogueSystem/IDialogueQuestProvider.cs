public interface IDialogueQuestProvider
{
    bool IsQuestStateMatched(string questId, int requiredStateValue);
}