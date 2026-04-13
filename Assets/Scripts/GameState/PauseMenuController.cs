using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class PauseMenuController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject saveLoadMenuRoot;

    [Header("Selection")]
    [SerializeField] private GameObject pauseFirstSelected;
    [SerializeField] private GameObject saveLoadFirstSelected;

    [Header("Optional Visuals")]
    [SerializeField] private Behaviour grayscaleEffect;
    [SerializeField] private CanvasGroupFader fadeFader;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameInput gameInput;
    private bool isSubscribed;
    private bool isTransitioning;

    private bool IsPauseOpen => pauseMenuRoot != null && pauseMenuRoot.activeSelf;
    private bool IsSaveLoadOpen => saveLoadMenuRoot != null && saveLoadMenuRoot.activeSelf;

    private void Awake()
    {
        SetPauseVisible(false);
        SetSaveLoadVisible(false);

        if (grayscaleEffect != null)
            grayscaleEffect.enabled = false;
    }

    private void OnEnable()
    {
        CacheInput();
        Subscribe();
    }

    private void Start()
    {
        CacheInput();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void CacheInput()
    {
        if (gameInput == null)
            gameInput = GameInput.Instance;
    }

    private void Subscribe()
    {
        if (isSubscribed || gameInput == null)
            return;

        gameInput.OnPausePressed += HandlePausePressed;
        gameInput.OnMenuCancelPressed += HandleMenuCancelPressed;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || gameInput == null)
            return;

        gameInput.OnPausePressed -= HandlePausePressed;
        gameInput.OnMenuCancelPressed -= HandleMenuCancelPressed;
        isSubscribed = false;
    }

    private void HandlePausePressed()
    {
        if (isTransitioning)
            return;

        if (IsPauseOpen)
            return;

        OpenPause();
    }

    private void HandleMenuCancelPressed()
    {
        if (isTransitioning)
            return;

        if (!IsPauseOpen)
            return;

        if (IsSaveLoadOpen)
        {
            CloseSaveLoad();
            return;
        }

        ResumeGame();
    }

    public void OpenPause()
    {
        Time.timeScale = 0f;
        SetPauseVisible(true);
        SetSaveLoadVisible(false);

        if (grayscaleEffect != null)
            grayscaleEffect.enabled = true;

        gameInput?.SetMenuMode();
        Select(pauseFirstSelected != null ? pauseFirstSelected : pauseMenuRoot);
    }

    public void ResumeGame()
    {
        SetSaveLoadVisible(false);
        SetPauseVisible(false);

        if (grayscaleEffect != null)
            grayscaleEffect.enabled = false;

        Time.timeScale = 1f;
        gameInput?.SetGameplayMode();
    }

    public void OpenSaveLoad()
    {
        if (!IsPauseOpen)
            return;

        SetSaveLoadVisible(true);
        Select(saveLoadFirstSelected != null ? saveLoadFirstSelected : saveLoadMenuRoot);
    }

    public void CloseSaveLoad()
    {
        SetSaveLoadVisible(false);
        Select(pauseFirstSelected != null ? pauseFirstSelected : pauseMenuRoot);
    }

    public void ReturnToMainMenu()
    {
        if (isTransitioning)
            return;

        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        if (fadeFader != null)
            yield return fadeFader.FadeTo(1f, fadeDuration, true);

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("[PauseMenuController] Main menu scene name is empty.", this);
            isTransitioning = false;
            yield break;
        }

        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private void SetPauseVisible(bool visible)
    {
        if (pauseMenuRoot == null)
            return;

        pauseMenuRoot.SetActive(visible);
    }

    private void SetSaveLoadVisible(bool visible)
    {
        if (saveLoadMenuRoot == null)
            return;

        saveLoadMenuRoot.SetActive(visible);

        if (visible)
            saveLoadMenuRoot.SendMessage("Open", SendMessageOptions.DontRequireReceiver);
        else
            saveLoadMenuRoot.SendMessage("Close", SendMessageOptions.DontRequireReceiver);
    }

    private static void Select(GameObject target)
    {
        if (target == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }
}