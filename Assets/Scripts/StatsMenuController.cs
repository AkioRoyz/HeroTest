using UnityEngine;

public class StatsMenuController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private EquipmentMenuUI equipmentMenuUI;
    [SerializeField] private StatsControlsHintUI statsControlsHintUI;

    private bool isOpened;

    private void Start()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnStats += ToggleMenu;
            gameInput.OnMenuClose += CloseMenu;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnStats -= ToggleMenu;
            gameInput.OnMenuClose -= CloseMenu;
        }
    }

    private void ToggleMenu()
    {
        if (gameInput.CurrentMode == GameInput.InputMode.Dialogue)
            return;

        if (isOpened)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (menuRoot == null || gameInput == null)
            return;

        isOpened = true;
        menuRoot.SetActive(true);
        gameInput.SwitchToMenuMode();

        if (equipmentMenuUI != null)
        {
            equipmentMenuUI.OpenMenu();
        }

        if (statsControlsHintUI != null)
        {
            statsControlsHintUI.Refresh();
        }
    }

    public void CloseMenu()
    {
        if (menuRoot == null || gameInput == null)
            return;

        isOpened = false;
        menuRoot.SetActive(false);
        gameInput.SwitchToPlayerMode();
    }
}