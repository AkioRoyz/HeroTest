public interface IDialogueQuestProvider
{
    // Возвращает true, если состояние квеста подходит.
    bool IsQuestStateMatched(string questId, int requiredState);
}