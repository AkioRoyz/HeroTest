using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("Start Game")]
    [SerializeField] private string firstGameplaySceneName = "SampleScene";
    [SerializeField] private string startEntryPointId;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isTransitioning;

    private void Awake()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    public void StartNewGame()
    {
        if (isTransitioning) return;
        StartCoroutine(StartNewGameRoutine());
    }

    private IEnumerator StartNewGameRoutine()
    {
        isTransitioning = true;

        yield return FadeToBlack();

        // Даём кадру отрисоваться полностью чёрным
        yield return new WaitForEndOfFrame();

        LoadGameplayScene();
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private void LoadGameplayScene()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.StartNewGame(firstGameplaySceneName, startEntryPointId);
            return;
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(firstGameplaySceneName, startEntryPointId);
            return;
        }

        SceneTransitionState.SetNextEntryPoint(startEntryPointId);
        SceneManager.LoadScene(firstGameplaySceneName, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}