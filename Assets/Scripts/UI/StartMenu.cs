using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class StartMenu : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private string bootLocationSceneName = "Boot";

    [Header("Selection")]
    [SerializeField] private GameObject bootLocationButton;
    [SerializeField] private GameObject exitButton;

    [Header("Fade")]
    [SerializeField] private CanvasGroupFader fadeFader;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float additionalDelay = 0.03f;

    private bool isTransitioning;

    private void OnEnable()
    {
        Time.timeScale = 1f;

        if (GameInput.Instance != null)
            GameInput.Instance.SetMenuMode();

        if (fadeFader != null)
            fadeFader.SetInstant(0f);

        Select(bootLocationButton != null ? bootLocationButton : exitButton);
    }

    public void OpenBootLocation()
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(bootLocationSceneName))
        {
            Debug.LogError("[StartMenu] Boot scene name is empty.", this);
            return;
        }

        StartCoroutine(OpenBootLocationRoutine());
    }

    private IEnumerator OpenBootLocationRoutine()
    {
        isTransitioning = true;

        if (fadeFader != null)
            yield return fadeFader.FadeTo(1f, fadeDuration, true);

        if (additionalDelay > 0f)
            yield return new WaitForSecondsRealtime(additionalDelay);

        SceneManager.LoadScene(bootLocationSceneName, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private static void Select(GameObject target)
    {
        if (target == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }
}