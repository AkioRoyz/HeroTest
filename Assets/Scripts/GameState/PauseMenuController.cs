using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private Button resumeButton;

    private void Start()
    {
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnPauseToggle += HandlePauseToggle;
            gameInput.OnPauseMenuSelect += HandlePauseMenuSelect;
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnPauseToggle -= HandlePauseToggle;
            gameInput.OnPauseMenuSelect -= HandlePauseMenuSelect;
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
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
            ResumeGame();
        }
    }

    private void HandlePauseMenuSelect()
    {
        if (GameStateManager.Instance == null)
            return;

        if (GameStateManager.Instance.CurrentState != GameState.Pause)
            return;

        ResumeGame();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        bool isPause = newState == GameState.Pause;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(isPause);
        }

        if (isPause && resumeButton != null)
        {
            resumeButton.Select();
        }
    }

    public void ResumeGame()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.Playing);
        }

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }

        if (gameInput != null)
        {
            gameInput.SwitchToPlayerMode();
        }
    }

    private void OpenPauseMenu()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.Pause);
        }

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(true);
        }

        if (gameInput != null)
        {
            gameInput.SwitchToPauseMenuMode();
        }

        if (resumeButton != null)
        {
            resumeButton.Select();
        }
    }
}