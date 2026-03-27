using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestJournalController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private QuestJournalUI questJournalUI;

    private bool isOpened;

    private void Awake()
    {
        ResolveReferences();

        isOpened = false;
        if (questJournalUI != null)
            questJournalUI.Close();

        // Важно: здесь НЕ трогаем gameInput.SwitchToPlayerMode()
        // чтобы не упасть по порядку инициализации.
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ResolveReferences();

        if (gameInput == null)
            return;

        gameInput.OnQuestJournal += ToggleJournalFromPlayer;
        gameInput.OnQuestJournalUp += HandleJournalUp;
        gameInput.OnQuestJournalDown += HandleJournalDown;
        gameInput.OnQuestJournalSelect += HandleJournalSelect;
        gameInput.OnQuestJournalBack += HandleJournalBack;
        gameInput.OnQuestJournalMainTab += HandleMainTab;
        gameInput.OnQuestJournalSideTab += HandleSideTab;
        gameInput.OnQuestJournalPinQuest += HandlePinQuest;
        gameInput.OnQuestJournalClose += HandleJournalClose;

        ForceCloseJournalVisual();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (gameInput == null)
            return;

        gameInput.OnQuestJournal -= ToggleJournalFromPlayer;
        gameInput.OnQuestJournalUp -= HandleJournalUp;
        gameInput.OnQuestJournalDown -= HandleJournalDown;
        gameInput.OnQuestJournalSelect -= HandleJournalSelect;
        gameInput.OnQuestJournalBack -= HandleJournalBack;
        gameInput.OnQuestJournalMainTab -= HandleMainTab;
        gameInput.OnQuestJournalSideTab -= HandleSideTab;
        gameInput.OnQuestJournalPinQuest -= HandlePinQuest;
        gameInput.OnQuestJournalClose -= HandleJournalClose;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveReferences();
        ForceCloseJournalVisual();
    }

    private void ResolveReferences()
    {
        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();
    }

    private void ForceCloseJournalVisual()
    {
        isOpened = false;

        if (questJournalUI != null)
            questJournalUI.Close();

        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Menu)
            GameStateManager.Instance.SetState(GameState.Playing);

        // Переключаем режим только когда контроллер уже активен
        // и ссылки точно резолвнуты.
        if (isActiveAndEnabled && gameInput != null)
            gameInput.SwitchToPlayerMode();
    }

    private void ToggleJournalFromPlayer()
    {
        if (gameInput == null || questJournalUI == null)
            return;

        if (GameStateManager.Instance != null)
        {
            if (GameStateManager.Instance.CurrentState == GameState.Dialogue ||
                GameStateManager.Instance.CurrentState == GameState.Pause)
            {
                return;
            }
        }

        if (gameInput.CurrentMode == GameInput.InputMode.Menu)
            return;

        if (isOpened) CloseJournal();
        else OpenJournal();
    }

    public void OpenJournal()
    {
        if (gameInput == null || questJournalUI == null)
            return;

        if (isOpened)
            return;

        isOpened = true;
        questJournalUI.Open();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Menu);

        gameInput.SwitchToQuestJournalMode();
    }

    public void CloseJournal()
    {
        if (gameInput == null || questJournalUI == null)
            return;

        if (!isOpened)
            return;

        isOpened = false;
        questJournalUI.Close();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Playing);

        gameInput.SwitchToPlayerMode();
    }

    private void HandleJournalUp() { if (isOpened && questJournalUI != null) questJournalUI.MoveSelectionUp(); }
    private void HandleJournalDown() { if (isOpened && questJournalUI != null) questJournalUI.MoveSelectionDown(); }
    private void HandleJournalSelect() { if (isOpened && questJournalUI != null) questJournalUI.HandleJournalSelectInput(); }
    private void HandleJournalBack() { if (isOpened && questJournalUI != null) questJournalUI.HandleJournalBackInput(); }
    private void HandleMainTab() { if (isOpened && questJournalUI != null) questJournalUI.SelectMainTab(); }
    private void HandleSideTab() { if (isOpened && questJournalUI != null) questJournalUI.SelectSideTab(); }
    private void HandlePinQuest() { if (isOpened && questJournalUI != null) questJournalUI.TogglePinForSelectedQuest(); }
    private void HandleJournalClose() { if (isOpened) CloseJournal(); }
}