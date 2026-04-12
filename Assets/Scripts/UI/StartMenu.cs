using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Button bootLocationButton;
    [SerializeField] private Button exitButton;

    [Header("Scene")]
    [SerializeField] private string bootLocationSceneName = "Boot";

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Navigation")]
    [SerializeField] private bool wrapSelection = true;

    private readonly List<Button> menuButtons = new();
    private GameInput subscribedInput;
    private int currentIndex;
    private bool isTransitioning;

    private void Awake()
    {
        ResolveReferences();
        CacheButtons();
        PrepareFadeCanvas();
    }

    private void OnEnable()
    {
        ResolveReferences();
        CacheButtons();
        RebindInput();
        EnterMainMenuMode();
    }

    private void OnDisable()
    {
        UnbindInput();
    }

    private void ResolveReferences()
    {
        gameInput = GameInput.Instance != null
            ? GameInput.Instance
            : FindFirstObjectByType<GameInput>();
    }

    private void CacheButtons()
    {
        menuButtons.Clear();

        if (bootLocationButton != null)
            menuButtons.Add(bootLocationButton);

        if (exitButton != null)
            menuButtons.Add(exitButton);

        if (currentIndex < 0 || currentIndex >= menuButtons.Count)
            currentIndex = 0;
    }

    private void PrepareFadeCanvas()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    private void RebindInput()
    {
        UnbindInput();

        if (gameInput == null)
            return;

        gameInput.OnMenuUp += HandleMenuUp;
        gameInput.OnMenuDown += HandleMenuDown;
        gameInput.OnMenuSelect += HandleMenuSelect;

        subscribedInput = gameInput;
    }

    private void UnbindInput()
    {
        if (subscribedInput == null)
            return;

        subscribedInput.OnMenuUp -= HandleMenuUp;
        subscribedInput.OnMenuDown -= HandleMenuDown;
        subscribedInput.OnMenuSelect -= HandleMenuSelect;

        subscribedInput = null;
    }

    private void EnterMainMenuMode()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Menu);
        else
            Time.timeScale = 0f;

        if (gameInput != null)
            gameInput.SwitchToMenuMode();

        SelectIndex(0, true);
    }

    private void HandleMenuUp()
    {
        if (isTransitioning || menuButtons.Count == 0)
            return;

        MoveSelection(-1);
    }

    private void HandleMenuDown()
    {
        if (isTransitioning || menuButtons.Count == 0)
            return;

        MoveSelection(1);
    }

    private void HandleMenuSelect()
    {
        if (isTransitioning || menuButtons.Count == 0)
            return;

        Button selectedButton = GetCurrentButton();
        if (selectedButton == null || !selectedButton.interactable)
            return;

        selectedButton.onClick.Invoke();
    }

    private void MoveSelection(int direction)
    {
        if (menuButtons.Count == 0)
            return;

        int nextIndex = currentIndex + direction;

        if (wrapSelection)
        {
            if (nextIndex < 0)
                nextIndex = menuButtons.Count - 1;
            else if (nextIndex >= menuButtons.Count)
                nextIndex = 0;
        }
        else
        {
            nextIndex = Mathf.Clamp(nextIndex, 0, menuButtons.Count - 1);
        }

        SelectIndex(nextIndex, false);
    }

    private void SelectIndex(int index, bool force)
    {
        if (menuButtons.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, menuButtons.Count - 1);

        if (!force && currentIndex == index)
            return;

        currentIndex = index;

        Button selectedButton = GetCurrentButton();
        if (selectedButton != null)
            selectedButton.Select();
    }

    private Button GetCurrentButton()
    {
        if (currentIndex < 0 || currentIndex >= menuButtons.Count)
            return null;

        return menuButtons[currentIndex];
    }

    public void OpenBootLocation()
    {
        if (isTransitioning)
            return;

        StartCoroutine(OpenBootLocationRoutine());
    }

    private IEnumerator OpenBootLocationRoutine()
    {
        isTransitioning = true;

        yield return FadeToBlack();
        yield return new WaitForEndOfFrame();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Playing);
        else
            Time.timeScale = 1f;

        LoadBootLocationScene();
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

    private void LoadBootLocationScene()
    {
        if (string.IsNullOrWhiteSpace(bootLocationSceneName))
        {
            Debug.LogError("[StartMenu] bootLocationSceneName is empty.", this);
            return;
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(bootLocationSceneName);
            return;
        }

        SceneManager.LoadScene(bootLocationSceneName, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}