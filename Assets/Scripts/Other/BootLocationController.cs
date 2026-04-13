using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class BootLocationController : MonoBehaviour
{
    [Header("Start Game")]
    [SerializeField] private string firstGameplaySceneName = "SampleScene";
    [SerializeField] private string startEntryPointId;

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Optional Save/Load Window")]
    [SerializeField] private GameObject saveLoadMenuRoot;
    [SerializeField] private GameObject saveLoadFirstSelected;

    [Header("Fade")]
    [SerializeField] private CanvasGroupFader fadeFader;
    [SerializeField] private float fadeDuration = 0.2f;

    private GameInput gameInput;
    private bool isSubscribed;
    private bool isTransitioning;

    private bool IsSaveLoadOpen => saveLoadMenuRoot != null && saveLoadMenuRoot.activeSelf;

    private void Awake()
    {
        SetSaveLoadVisible(false);
    }

    private void OnEnable()
    {
        CacheInput();
        Subscribe();

        if (gameInput != null)
            gameInput.SetBootExploreMode();
    }

    private void Start()
    {
        CacheInput();

        if (gameInput != null)
            gameInput.SetBootExploreMode();
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

        gameInput.OnMenuCancelPressed += HandleMenuCancelPressed;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || gameInput == null)
            return;

        gameInput.OnMenuCancelPressed -= HandleMenuCancelPressed;
        isSubscribed = false;
    }

    private void HandleMenuCancelPressed()
    {
        if (isTransitioning)
            return;

        if (IsSaveLoadOpen)
            CloseLoadMenu();
    }

    public void StartNewGame()
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(firstGameplaySceneName))
        {
            Debug.LogError("[BootLocationController] First gameplay scene name is empty.", this);
            return;
        }

        StartCoroutine(StartNewGameRoutine());
    }

    private IEnumerator StartNewGameRoutine()
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        if (fadeFader != null)
            yield return fadeFader.FadeTo(1f, fadeDuration, true);

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.StartNewGame(firstGameplaySceneName, startEntryPointId);
            yield break;
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(firstGameplaySceneName, startEntryPointId);
            yield break;
        }

        SceneTransitionState.SetNextEntryPoint(startEntryPointId);
        SceneManager.LoadScene(firstGameplaySceneName, LoadSceneMode.Single);
    }

    public void OpenLoadMenu()
    {
        if (isTransitioning)
            return;

        SetSaveLoadVisible(true);
        gameInput?.SetMenuMode();

        Select(saveLoadFirstSelected != null ? saveLoadFirstSelected : saveLoadMenuRoot);
    }

    public void CloseLoadMenu()
    {
        SetSaveLoadVisible(false);
        gameInput?.SetBootExploreMode();
    }

    public void ReturnToMainMenu()
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("[BootLocationController] Main menu scene name is empty.", this);
            return;
        }

        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        if (fadeFader != null)
            yield return fadeFader.FadeTo(1f, fadeDuration, true);

        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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