using UnityEngine;

public class QuestJournalController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private QuestJournalUI questJournalUI;

    private bool isOpened;

    private void Awake()
    {
        if (questJournalUI != null)
        {
            questJournalUI.Close();
        }

        isOpened = false;
    }

    private void OnEnable()
    {
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
    }

    private void OnDisable()
    {
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

        if (isOpened)
        {
            CloseJournal();
        }
        else
        {
            OpenJournal();
        }
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
        {
            GameStateManager.Instance.SetState(GameState.Menu);
        }

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
        {
            GameStateManager.Instance.SetState(GameState.Playing);
        }

        gameInput.SwitchToPlayerMode();
    }

    private void HandleJournalUp()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.MoveSelectionUp();
    }

    private void HandleJournalDown()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.MoveSelectionDown();
    }

    private void HandleJournalSelect()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.HandleJournalSelectInput();
    }

    private void HandleJournalBack()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.HandleJournalBackInput();
    }

    private void HandleMainTab()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.SelectMainTab();
    }

    private void HandleSideTab()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.SelectSideTab();
    }

    private void HandlePinQuest()
    {
        if (!isOpened || questJournalUI == null)
            return;

        questJournalUI.TogglePinForSelectedQuest();
    }

    private void HandleJournalClose()
    {
        if (!isOpened)
            return;

        CloseJournal();
    }
}