using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private Button resumeButton;
    [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;

    [Header("Pause Visual Effect")]
    [SerializeField] private Volume pauseGrayScaleVolume;
    [SerializeField, Range(0f, 1f)] private float pauseGrayScaleWeight = 1f;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameInput subscribedInput;
    private bool suppressPauseReturnFromSaveWindowClose;

    private void Awake()
    {
        ResolveReferences();
        ForceClosedVisual();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;

        ResolveReferences();
        RebindInput();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        if (saveLoadMenuUI != null)
            saveLoadMenuUI.OnWindowClosed += HandleSaveLoadWindowClosed;

        ForceClosedVisual();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindInput();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;

        if (saveLoadMenuUI != null)
            saveLoadMenuUI.OnWindowClosed -= HandleSaveLoadWindowClosed;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveReferences();
        RebindInput();
        ForceClosedVisual();
    }

    private void ResolveReferences()
    {
        gameInput = GameInput.Instance != null ? GameInput.Instance : FindFirstObjectByType<GameInput>();
    }

    private void RebindInput()
    {
        UnbindInput();

        if (gameInput == null)
            return;

        gameInput.OnPauseToggle += HandlePauseToggle;
        gameInput.OnPauseMenuSelect += HandlePauseMenuSelect;
        subscribedInput = gameInput;
    }

    private void UnbindInput()
    {
        if (subscribedInput == null)
            return;

        subscribedInput.OnPauseToggle -= HandlePauseToggle;
        subscribedInput.OnPauseMenuSelect -= HandlePauseMenuSelect;
        subscribedInput = null;
    }

    private void ForceClosedVisual()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        CloseSaveLoadSilently();

        if (pauseGrayScaleVolume != null)
            pauseGrayScaleVolume.weight = 0f;

        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Pause)
            GameStateManager.Instance.SetState(GameState.Playing);

        if (gameInput != null)
            gameInput.SwitchToPlayerMode();
    }

    private void HandlePauseToggle()
    {
        if (GameStateManager.Instance == null || gameInput == null)
            return;

        if (GameStateManager.Instance.CurrentState == GameState.Playing)
        {
            OpenPauseMenu();
        }
        else if (GameStateManager.Instance.CurrentState == GameState.Pause)
        {
            if (saveLoadMenuUI != null && saveLoadMenuUI.IsOpen)
                saveLoadMenuUI.Close();
            else
                ResumeGame();
        }
    }

    private void HandlePauseMenuSelect()
    {
        if (GameStateManager.Instance == null)
            return;

        if (GameStateManager.Instance.CurrentState != GameState.Pause)
            return;

        if (saveLoadMenuUI != null && saveLoadMenuUI.IsOpen)
            return;

        ResumeGame();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        bool isPause = newState == GameState.Pause;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(isPause);

        if (pauseGrayScaleVolume != null)
            pauseGrayScaleVolume.weight = isPause ? pauseGrayScaleWeight : 0f;

        if (!isPause)
            CloseSaveLoadSilently();

        if (isPause && resumeButton != null)
            resumeButton.Select();
    }

    private void HandleSaveLoadWindowClosed()
    {
        if (suppressPauseReturnFromSaveWindowClose)
            return;

        if (GameStateManager.Instance == null)
            return;

        if (GameStateManager.Instance.CurrentState != GameState.Pause)
            return;

        if (gameInput != null)
            gameInput.SwitchToPauseMenuMode();

        if (resumeButton != null)
            resumeButton.Select();
    }

    private void CloseSaveLoadSilently()
    {
        if (saveLoadMenuUI == null || !saveLoadMenuUI.IsOpen)
            return;

        suppressPauseReturnFromSaveWindowClose = true;
        saveLoadMenuUI.Close();
        suppressPauseReturnFromSaveWindowClose = false;
    }

    public void ResumeGame()
    {
        CloseSaveLoadSilently();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Playing);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (pauseGrayScaleVolume != null)
            pauseGrayScaleVolume.weight = 0f;

        if (gameInput != null)
            gameInput.SwitchToPlayerMode();
    }

    public void OpenSaveMenu()
    {
        if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.Pause)
            return;

        if (saveLoadMenuUI == null)
        {
            Debug.LogWarning("[PauseMenuController] SaveLoadMenuUI is not assigned.", this);
            return;
        }

        saveLoadMenuUI.OpenSaveMode();

        if (gameInput != null)
            gameInput.SwitchToMenuMode();
    }

    public void OpenLoadMenu()
    {
        if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.Pause)
            return;

        if (saveLoadMenuUI == null)
        {
            Debug.LogWarning("[PauseMenuController] SaveLoadMenuUI is not assigned.", this);
            return;
        }

        saveLoadMenuUI.OpenLoadMode();

        if (gameInput != null)
            gameInput.SwitchToMenuMode();
    }

    public void CloseSaveLoadWindow()
    {
        if (saveLoadMenuUI == null)
            return;

        saveLoadMenuUI.Close();
    }

    public void ReturnToMainMenu()
    {
        CloseSaveLoadSilently();

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (pauseGrayScaleVolume != null)
            pauseGrayScaleVolume.weight = 0f;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Menu);

        if (gameInput != null)
            gameInput.SwitchToMenuMode();

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private void OpenPauseMenu()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Pause);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        if (pauseGrayScaleVolume != null)
            pauseGrayScaleVolume.weight = pauseGrayScaleWeight;

        if (gameInput != null)
            gameInput.SwitchToPauseMenuMode();

        if (resumeButton != null)
            resumeButton.Select();
    }
}