using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class QuestNotificationController : MonoBehaviour
{
    private class NotificationRequest
    {
        public QuestNotificationUI.NotificationType Type;
        public bool UseLocalizedTitle;
        public string PlainTitle;
        public LocalizedString LocalizedTitle;
        public string FallbackTitle;
    }

    [Header("UI")]
    [SerializeField] private QuestNotificationUI notificationUI;

    private readonly Queue<NotificationRequest> notificationQueue = new();
    private Coroutine processRoutine;

    private void Awake()
    {
        if (notificationUI == null)
        {
            notificationUI = GetComponentInChildren<QuestNotificationUI>(true);
        }

        if (notificationUI != null)
        {
            notificationUI.HideImmediate();
        }
    }

    private void OnDisable()
    {
        if (processRoutine != null)
        {
            StopCoroutine(processRoutine);
            processRoutine = null;
        }
    }

    public void QueueAccepted(string questTitle)
    {
        Enqueue(new NotificationRequest
        {
            Type = QuestNotificationUI.NotificationType.Accepted,
            UseLocalizedTitle = false,
            PlainTitle = questTitle ?? string.Empty,
            FallbackTitle = questTitle ?? string.Empty
        });
    }

    public void QueueCompleted(string questTitle)
    {
        Enqueue(new NotificationRequest
        {
            Type = QuestNotificationUI.NotificationType.Completed,
            UseLocalizedTitle = false,
            PlainTitle = questTitle ?? string.Empty,
            FallbackTitle = questTitle ?? string.Empty
        });
    }

    public void QueueAccepted(LocalizedString localizedQuestTitle, string fallbackTitle = "")
    {
        Enqueue(new NotificationRequest
        {
            Type = QuestNotificationUI.NotificationType.Accepted,
            UseLocalizedTitle = true,
            LocalizedTitle = localizedQuestTitle,
            FallbackTitle = fallbackTitle ?? string.Empty
        });
    }

    public void QueueCompleted(LocalizedString localizedQuestTitle, string fallbackTitle = "")
    {
        Enqueue(new NotificationRequest
        {
            Type = QuestNotificationUI.NotificationType.Completed,
            UseLocalizedTitle = true,
            LocalizedTitle = localizedQuestTitle,
            FallbackTitle = fallbackTitle ?? string.Empty
        });
    }

    public void Queue(QuestNotificationUI.NotificationType type, string questTitle)
    {
        Enqueue(new NotificationRequest
        {
            Type = type,
            UseLocalizedTitle = false,
            PlainTitle = questTitle ?? string.Empty,
            FallbackTitle = questTitle ?? string.Empty
        });
    }

    public void Queue(QuestNotificationUI.NotificationType type, LocalizedString localizedQuestTitle, string fallbackTitle = "")
    {
        Enqueue(new NotificationRequest
        {
            Type = type,
            UseLocalizedTitle = true,
            LocalizedTitle = localizedQuestTitle,
            FallbackTitle = fallbackTitle ?? string.Empty
        });
    }

    // Алиасы на случай, если старый код вызывал похожие методы
    public void EnqueueAccepted(string questTitle) => QueueAccepted(questTitle);
    public void EnqueueCompleted(string questTitle) => QueueCompleted(questTitle);

    public void EnqueueAccepted(LocalizedString localizedQuestTitle, string fallbackTitle = "")
        => QueueAccepted(localizedQuestTitle, fallbackTitle);

    public void EnqueueCompleted(LocalizedString localizedQuestTitle, string fallbackTitle = "")
        => QueueCompleted(localizedQuestTitle, fallbackTitle);

    public void EnqueueNotification(QuestNotificationUI.NotificationType type, string questTitle)
        => Queue(type, questTitle);

    public void EnqueueNotification(QuestNotificationUI.NotificationType type, LocalizedString localizedQuestTitle, string fallbackTitle = "")
        => Queue(type, localizedQuestTitle, fallbackTitle);

    private void Enqueue(NotificationRequest request)
    {
        if (request == null)
            return;

        notificationQueue.Enqueue(request);

        if (processRoutine == null && isActiveAndEnabled)
        {
            processRoutine = StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (notificationQueue.Count > 0)
        {
            if (notificationUI == null)
            {
                Debug.LogWarning("[QuestNotificationController] Notification UI is missing.", this);
                processRoutine = null;
                yield break;
            }

            NotificationRequest request = notificationQueue.Dequeue();

            if (request.UseLocalizedTitle)
            {
                if (request.Type == QuestNotificationUI.NotificationType.Accepted)
                    notificationUI.ShowAccepted(request.LocalizedTitle, request.FallbackTitle);
                else
                    notificationUI.ShowCompleted(request.LocalizedTitle, request.FallbackTitle);
            }
            else
            {
                if (request.Type == QuestNotificationUI.NotificationType.Accepted)
                    notificationUI.ShowAccepted(request.PlainTitle);
                else
                    notificationUI.ShowCompleted(request.PlainTitle);
            }

            float waitTime = notificationUI.GetTotalDisplayDuration();
            yield return WaitUnscaled(waitTime);
        }

        processRoutine = null;
    }

    private IEnumerator WaitUnscaled(float duration)
    {
        if (duration <= 0f)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}