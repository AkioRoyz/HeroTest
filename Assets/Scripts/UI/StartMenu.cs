using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("Start Game")]
    [SerializeField] private string firstGameplaySceneName = "SampleScene";
    [SerializeField] private string startEntryPointId;

    [Header("Save UI")]
    [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;
    [SerializeField] private GameObject continueButtonRoot;

    private void OnEnable()
    {
        RefreshContinueButton();

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnSaveSlotsChanged += RefreshContinueButton;
    }

    private void OnDisable()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnSaveSlotsChanged -= RefreshContinueButton;
    }

    public void StartNewGame()
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

    public void StartTest()
    {
        StartNewGame();
    }

    public void OpenContinueMenu()
    {
        if (saveLoadMenuUI != null)
            saveLoadMenuUI.OpenLoadMode();
    }

    public void RefreshContinueButton()
    {
        if (continueButtonRoot == null)
            return;

        bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasAnySave();
        continueButtonRoot.SetActive(hasSave);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}