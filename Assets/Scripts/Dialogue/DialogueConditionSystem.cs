using UnityEngine;

public class DialogueConditionSystem : MonoBehaviour
{
    public bool CheckCondition(DialogueConditionType type, string id)
    {
        switch (type)
        {
            case DialogueConditionType.QuestActive:
                return IsQuestActive(id);

            case DialogueConditionType.QuestCompleted:
                return IsQuestCompleted(id);

            case DialogueConditionType.QuestNotStarted:
                return !IsQuestActive(id);

            default:
                return true;
        }
    }


    bool IsQuestActive(string id)
    {
        Debug.Log("Check Quest Active: " + id);
        return true;
    }

    bool IsQuestCompleted(string id)
    {
        Debug.Log("Check Quest Completed: " + id);
        return false;
    }
}