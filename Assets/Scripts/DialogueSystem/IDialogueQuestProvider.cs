public interface IDialogueQuestProvider
{
    bool IsQuestStateMatched(string questId, int requiredQuestState);
    bool IsQuestStepIndexMatched(string questId, int requiredQuestStepIndex);
}