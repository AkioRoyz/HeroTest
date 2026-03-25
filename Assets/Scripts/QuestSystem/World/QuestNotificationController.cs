using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestNotificationController : MonoBehaviour
{
    private class NotificationRequest
    {
        public QuestNotificationUI.NotificationType Type;
        public string QuestTitle;

        public NotificationRequest(QuestNotificationUI.NotificationType type, string questTitle)
        {
            Type = type;
            QuestTitle = questTitle;
        }
    }

    [Header("References")]
    [SerializeField] private QuestNotificationUI notificationUI;

    private readonly Queue<NotificationRequest> queue = new();
    private Coroutine queueRoutine;
    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribeQuestManager();
    }

    private void Start()
    {
        TrySubscribeQuestManager();

        if (notificationUI != null)
        {
            notificationUI.HideImmediate();
        }
    }

    private void OnDisable()
    {
        UnsubscribeQuestManager();
    }

    private void TrySubscribeQuestManager()
    {
        if (isSubscribed)
            return;

        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestAccepted += HandleQuestAccepted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        isSubscribed = true;
    }

    private void UnsubscribeQuestManager()
    {
        if (!isSubscribed)
            return;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= HandleQuestAccepted;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }

        isSubscribed = false;
    }

    private void HandleQuestAccepted(QuestData questData)
    {
        if (questData == null)
            return;

        if (!questData.NotifyOnAccept)
            return;

        EnqueueNotification(
            QuestNotificationUI.NotificationType.Accepted,
            GetQuestTitle(questData)
        );
    }

    private void HandleQuestCompleted(QuestData questData)
    {
        if (questData == null)
            return;

        if (!questData.NotifyOnComplete)
            return;

        EnqueueNotification(
            QuestNotificationUI.NotificationType.Completed,
            GetQuestTitle(questData)
        );
    }

    private void EnqueueNotification(QuestNotificationUI.NotificationType type, string questTitle)
    {
        if (notificationUI == null)
            return;

        queue.Enqueue(new NotificationRequest(type, questTitle));

        if (queueRoutine == null)
        {
            queueRoutine = StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (queue.Count > 0)
        {
            NotificationRequest request = queue.Dequeue();

            if (notificationUI != null)
            {
                notificationUI.Show(request.Type, request.QuestTitle);

                float duration = notificationUI.GetTotalDisplayDuration();
                if (duration > 0f)
                {
                    yield return WaitUnscaled(duration + 0.05f);
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }

        queueRoutine = null;
    }

    private IEnumerator WaitUnscaled(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private string GetQuestTitle(QuestData questData)
    {
        if (questData == null)
            return string.Empty;

        if (questData.QuestTitle == null || questData.QuestTitle.IsEmpty)
            return questData.QuestId;

        var handle = questData.QuestTitle.GetLocalizedStringAsync();
        string result = handle.WaitForCompletion();

        if (!string.IsNullOrEmpty(result))
            return result;

        return questData.QuestId;
    }
}