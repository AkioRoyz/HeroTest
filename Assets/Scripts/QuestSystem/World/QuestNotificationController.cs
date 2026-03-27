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
    private bool isSubscribedToQuestManager;
    private bool isSubscribedToGameState;

    private void OnEnable()
    {
        TrySubscribeQuestManager();
        TrySubscribeGameStateManager();
    }

    private void Start()
    {
        TrySubscribeQuestManager();
        TrySubscribeGameStateManager();

        if (notificationUI != null)
        {
            notificationUI.HideImmediate();
        }
    }

    private void OnDisable()
    {
        UnsubscribeQuestManager();
        UnsubscribeGameStateManager();
    }

    private void TrySubscribeQuestManager()
    {
        if (isSubscribedToQuestManager)
            return;

        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestAccepted += HandleQuestAccepted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        isSubscribedToQuestManager = true;
    }

    private void UnsubscribeQuestManager()
    {
        if (!isSubscribedToQuestManager)
            return;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= HandleQuestAccepted;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }

        isSubscribedToQuestManager = false;
    }

    private void TrySubscribeGameStateManager()
    {
        if (isSubscribedToGameState)
            return;

        if (GameStateManager.Instance == null)
            return;

        GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        isSubscribedToGameState = true;
    }

    private void UnsubscribeGameStateManager()
    {
        if (!isSubscribedToGameState)
            return;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        isSubscribedToGameState = false;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState != GameState.Playing)
            return;

        TryStartQueueRoutine();
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
        TryStartQueueRoutine();
    }

    private void TryStartQueueRoutine()
    {
        if (queueRoutine != null)
            return;

        if (queue.Count == 0)
            return;

        if (!IsGameInPlayingState())
            return;

        queueRoutine = StartCoroutine(ProcessQueueRoutine());
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (queue.Count > 0)
        {
            yield return WaitUntilPlayingState();

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

        if (queue.Count > 0 && IsGameInPlayingState())
        {
            queueRoutine = StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator WaitUntilPlayingState()
    {
        while (!IsGameInPlayingState())
        {
            yield return null;
        }
    }

    private bool IsGameInPlayingState()
    {
        if (GameStateManager.Instance == null)
            return true;

        return GameStateManager.Instance.CurrentState == GameState.Playing;
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