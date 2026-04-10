using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLocationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;

    [Header("Start Game")]
    [SerializeField] private string firstGameplaySceneName = "SampleScene";
    [SerializeField] private string startEntryPointId;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private GameInput subscribedInput;
    private BootActionTrigger currentAvailableTrigger;
    private bool isTransitioning;
    private bool isLoadWindowOpen;

    private void Awake()
    {
        ResolveReferences();
        PrepareFadeCanvas();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RebindInput();

        if (saveLoadMenuUI != null)
            saveLoadMenuUI.OnWindowClosed += HandleLoadWindowClosed;

        if (gameInput != null)
            gameInput.SwitchToPlayerMode();
    }

    private void OnDisable()
    {
        if (saveLoadMenuUI != null)
            saveLoadMenuUI.OnWindowClosed -= HandleLoadWindowClosed;

        UnbindInput();
    }

    private void ResolveReferences()
    {
        if (gameInput == null)
            gameInput = GameInput.Instance != null ? GameInput.Instance : FindFirstObjectByType<GameInput>();
    }

    private void RebindInput()
    {
        UnbindInput();

        if (gameInput == null)
            return;

        gameInput.OnMenuClose += HandleMenuClose;
        subscribedInput = gameInput;
    }

    private void UnbindInput()
    {
        if (subscribedInput == null)
            return;

        subscribedInput.OnMenuClose -= HandleMenuClose;
        subscribedInput = null;
    }

    private void PrepareFadeCanvas()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    public void RegisterAvailableTrigger(BootActionTrigger trigger)
    {
        if (trigger == null)
            return;

        if (isTransitioning || isLoadWindowOpen)
            return;

        if (currentAvailableTrigger == trigger)
            return;

        ClearCurrentTriggerFocus();
        currentAvailableTrigger = trigger;
        currentAvailableTrigger.SetFocused(true);
    }

    public void UnregisterAvailableTrigger(BootActionTrigger trigger)
    {
        if (trigger == null)
            return;

        if (currentAvailableTrigger != trigger)
            return;

        currentAvailableTrigger.SetFocused(false);
        currentAvailableTrigger = null;
    }

    public void TryInteract()
    {
        if (isTransitioning)
            return;

        if (isLoadWindowOpen)
            return;

        if (currentAvailableTrigger == null)
            return;

        currentAvailableTrigger.Execute(this);
    }

    public void StartNewGame()
    {
        if (isTransitioning)
            return;

        StartCoroutine(StartNewGameRoutine());
    }

    public void OpenLoadMenu()
    {
        if (isTransitioning)
            return;

        if (saveLoadMenuUI == null)
        {
            Debug.LogWarning("[BootLocationController] SaveLoadMenuUI is not assigned.", this);
            return;
        }

        ClearCurrentTriggerFocus();

        saveLoadMenuUI.OpenLoadMode();
        isLoadWindowOpen = true;

        if (gameInput != null)
            gameInput.SwitchToMenuMode();
    }

    public void CloseLoadMenu()
    {
        if (saveLoadMenuUI == null)
            return;

        if (!saveLoadMenuUI.IsOpen)
            return;

        saveLoadMenuUI.Close();
    }

    public void ExitGame()
    {
        if (isTransitioning)
            return;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator StartNewGameRoutine()
    {
        isTransitioning = true;

        CloseLoadMenu();
        ClearCurrentTriggerFocus();

        yield return FadeToBlack();
        yield return new WaitForEndOfFrame();

        LoadGameplayScene();
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;

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

    private void HandleLoadWindowClosed()
    {
        isLoadWindowOpen = false;

        if (gameInput != null)
            gameInput.SwitchToPlayerMode();

        RestoreCurrentTriggerFocusIfPossible();
    }

    private void HandleMenuClose()
    {
        if (!isLoadWindowOpen)
            return;

        CloseLoadMenu();
    }

    private void ClearCurrentTriggerFocus()
    {
        if (currentAvailableTrigger != null)
            currentAvailableTrigger.SetFocused(false);
    }

    private void RestoreCurrentTriggerFocusIfPossible()
    {
        if (isTransitioning)
            return;

        if (currentAvailableTrigger != null)
            currentAvailableTrigger.SetFocused(true);
    }
}