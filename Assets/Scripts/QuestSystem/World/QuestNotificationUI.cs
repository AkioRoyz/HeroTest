using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class QuestNotificationUI : MonoBehaviour
{
    public enum NotificationType
    {
        Accepted,
        Completed
    }

    [Header("Visual Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI questTitleText;

    [Header("Localized Headers")]
    [SerializeField] private LocalizedString acceptedHeaderText;
    [SerializeField] private LocalizedString completedHeaderText;

    [Header("Animation")]
    [SerializeField] private float delayBeforeShow = 0.2f; // 👈 НОВОЕ
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float visibleDuration = 2.25f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void ShowAccepted(string questTitle)
    {
        Show(NotificationType.Accepted, questTitle);
    }

    public void ShowCompleted(string questTitle)
    {
        Show(NotificationType.Completed, questTitle);
    }

    public void Show(NotificationType type, string questTitle)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowRoutine(type, questTitle));
    }

    public float GetTotalDisplayDuration()
    {
        return delayBeforeShow + fadeInDuration + visibleDuration + fadeOutDuration;
    }

    public void HideImmediate()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }
    }

    private IEnumerator ShowRoutine(NotificationType type, string questTitle)
    {
        // 👉 ЗАДЕРЖКА ПЕРЕД ПОКАЗОМ
        yield return WaitUnscaled(delayBeforeShow);

        string header = type == NotificationType.Accepted
            ? GetLocalizedString(acceptedHeaderText, "Новый квест")
            : GetLocalizedString(completedHeaderText, "Квест завершён");

        if (headerText != null)
        {
            headerText.text = header;
        }

        if (questTitleText != null)
        {
            questTitleText.text = questTitle ?? string.Empty;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        yield return FadeCanvasGroup(0f, 1f, fadeInDuration);
        yield return WaitUnscaled(visibleDuration);
        yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }

        currentRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float timer = 0f;
        canvasGroup.alpha = from;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
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

    private string GetLocalizedString(LocalizedString localizedString, string fallback)
    {
        if (localizedString == null || localizedString.IsEmpty)
            return fallback ?? string.Empty;

        var handle = localizedString.GetLocalizedStringAsync();
        string result = handle.WaitForCompletion();

        if (!string.IsNullOrEmpty(result))
            return result;

        return fallback ?? string.Empty;
    }
}