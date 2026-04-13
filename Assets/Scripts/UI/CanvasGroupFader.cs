using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class CanvasGroupFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup target;
    [SerializeField] private bool blocksRaycastsWhenVisible = true;
    [SerializeField] private bool interactableWhenVisible = false;

    private Coroutine runningCoroutine;

    private void Reset()
    {
        target = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (target == null)
            target = GetComponent<CanvasGroup>();
    }

    public void SetInstant(float alpha)
    {
        if (target == null)
            return;

        target.alpha = Mathf.Clamp01(alpha);
        ApplyVisibilityFlags(target.alpha > 0.001f);
    }

    public Coroutine FadeTo(float targetAlpha, float duration, bool ignoreTimeScale = true)
    {
        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        runningCoroutine = StartCoroutine(FadeRoutine(Mathf.Clamp01(targetAlpha), Mathf.Max(0f, duration), ignoreTimeScale));
        return runningCoroutine;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool ignoreTimeScale)
    {
        if (target == null)
            yield break;

        float startAlpha = target.alpha;

        if (duration <= 0f)
        {
            target.alpha = targetAlpha;
            ApplyVisibilityFlags(target.alpha > 0.001f);
            runningCoroutine = null;
            yield break;
        }

        ApplyVisibilityFlags(true);

        float time = 0f;

        while (time < duration)
        {
            time += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            target.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        target.alpha = targetAlpha;
        ApplyVisibilityFlags(target.alpha > 0.001f);
        runningCoroutine = null;
    }

    private void ApplyVisibilityFlags(bool visible)
    {
        target.blocksRaycasts = blocksRaycastsWhenVisible && visible;
        target.interactable = interactableWhenVisible && visible;
    }
}