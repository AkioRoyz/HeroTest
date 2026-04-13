using UnityEngine;
using UnityEngine.EventSystems;

public sealed class StatsMenuController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject equipmentMenuRoot;
    [SerializeField] private GameObject controlsHintRoot;

    [Header("Selection")]
    [SerializeField] private GameObject menuFirstSelected;
    [SerializeField] private GameObject equipmentFirstSelected;

    private GameInput gameInput;
    private bool isSubscribed;

    private bool IsMenuOpen => menuRoot != null && menuRoot.activeSelf;
    private bool IsEquipmentOpen => equipmentMenuRoot != null && equipmentMenuRoot.activeSelf;

    private void Awake()
    {
        SetMenuVisible(false);
        SetEquipmentVisible(false);
        SetHintsVisible(false);
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

        gameInput.OnStatsPressed += HandleStatsPressed;
        gameInput.OnMenuCancelPressed += HandleMenuCancelPressed;
        gameInput.OnCloseEquipmentPressed += HandleCloseEquipmentPressed;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || gameInput == null)
            return;

        gameInput.OnStatsPressed -= HandleStatsPressed;
        gameInput.OnMenuCancelPressed -= HandleMenuCancelPressed;
        gameInput.OnCloseEquipmentPressed -= HandleCloseEquipmentPressed;
        isSubscribed = false;
    }

    private void HandleStatsPressed()
    {
        if (IsMenuOpen)
            return;

        OpenMenu();
    }

    private void HandleMenuCancelPressed()
    {
        if (!IsMenuOpen)
            return;

        if (CloseEquipmentIfOpen())
            return;

        CloseMenu();
    }

    private void HandleCloseEquipmentPressed()
    {
        if (!IsMenuOpen)
            return;

        CloseEquipmentIfOpen();
    }

    public void OpenMenu()
    {
        SetMenuVisible(true);
        SetHintsVisible(true);
        SetEquipmentVisible(false);

        gameInput?.SetMenuMode();
        Select(menuFirstSelected != null ? menuFirstSelected : menuRoot);
    }

    public void CloseMenu()
    {
        SetEquipmentVisible(false);
        SetHintsVisible(false);
        SetMenuVisible(false);

        gameInput?.SetGameplayMode();
    }

    public void OpenEquipmentMenu()
    {
        if (!IsMenuOpen)
            return;

        SetEquipmentVisible(true);
        Select(equipmentFirstSelected != null ? equipmentFirstSelected : equipmentMenuRoot);
    }

    public void CloseEquipmentMenu()
    {
        if (!IsMenuOpen)
            return;

        SetEquipmentVisible(false);
        Select(menuFirstSelected != null ? menuFirstSelected : menuRoot);
    }

    private bool CloseEquipmentIfOpen()
    {
        if (!IsEquipmentOpen)
            return false;

        CloseEquipmentMenu();
        return true;
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuRoot == null)
            return;

        menuRoot.SetActive(visible);
    }

    private void SetEquipmentVisible(bool visible)
    {
        if (equipmentMenuRoot == null)
            return;

        equipmentMenuRoot.SetActive(visible);

        if (visible)
            equipmentMenuRoot.SendMessage("Open", SendMessageOptions.DontRequireReceiver);
        else
            equipmentMenuRoot.SendMessage("Close", SendMessageOptions.DontRequireReceiver);
    }

    private void SetHintsVisible(bool visible)
    {
        if (controlsHintRoot == null)
            return;

        controlsHintRoot.SetActive(visible);
    }

    private static void Select(GameObject target)
    {
        if (target == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }
}