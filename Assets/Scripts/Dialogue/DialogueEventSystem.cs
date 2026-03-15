using UnityEngine;

public class DialogueEventSystem : MonoBehaviour
{
    public void TriggerEvent(DialogueEventType eventType, string eventID)
    {
        switch (eventType)
        {
            case DialogueEventType.StartQuest:
                StartQuest(eventID);
                break;

            case DialogueEventType.CompleteQuest:
                CompleteQuest(eventID);
                break;

            case DialogueEventType.GiveReward:
                GiveReward(eventID);
                break;
        }
    }


    void StartQuest(string questID)
    {
        Debug.Log("Start Quest: " + questID);
    }

    void CompleteQuest(string questID)
    {
        Debug.Log("Complete Quest: " + questID);
    }

    void GiveReward(string rewardID)
    {
        Debug.Log("Give Reward: " + rewardID);
    }
}