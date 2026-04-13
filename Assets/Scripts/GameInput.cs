using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameInput : MonoBehaviour
{
    public enum InputContext
    {
        Gameplay,
        BootExplore,
        MenuOnly,
        Cutscene
    }

    public static GameInput Instance { get; private set; }

    [Header("Input Asset")]
    [SerializeField] private InputActionAsset actions;

    [Header("Action Map Names")]
    [SerializeField] private string playerMapName = "Player";
    [SerializeField] private string menuMapName = "Menu";

    [Header("Player Actions")]
    [SerializeField] private string pauseActionName = "Pause";
    [SerializeField] private string statsActionName = "OpenStats";

    [Header("Menu Actions")]
    [SerializeField] private string menuCancelActionName = "CloseUI";
    [SerializeField] private string closeEquipmentActionName = "CloseEquipment";

    public InputContext CurrentContext { get; private set; } = InputContext.Gameplay;

    public event Action OnPausePressed;
    public event Action OnStatsPressed;
    public event Action OnMenuCancelPressed;
    public event Action OnCloseEquipmentPressed;

    private InputActionMap playerMap;
    private InputActionMap menuMap;

    private InputAction pauseAction;
    private InputAction statsAction;
    private InputAction menuCancelAction;
    private InputAction closeEquipmentAction;

    private bool isSubscribed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResolveActions();
        ApplyContext(CurrentContext);
    }

    private void OnEnable()
    {
        ResolveActions();
        Subscribe();
        ApplyContext(CurrentContext);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetGameplayMode()
    {
        SetContext(InputContext.Gameplay);
    }

    public void SetBootExploreMode()
    {
        SetContext(InputContext.BootExplore);
    }

    public void SetMenuMode()
    {
        SetContext(InputContext.MenuOnly);
    }

    public void SetCutsceneMode()
    {
        SetContext(InputContext.Cutscene);
    }

    public void SetContext(InputContext newContext)
    {
        CurrentContext = newContext;
        ApplyContext(CurrentContext);
    }

    private void ResolveActions()
    {
        if (actions == null)
        {
            Debug.LogError("[GameInput] InputActionAsset is not assigned.", this);
            return;
        }

        playerMap = actions.FindActionMap(playerMapName, false);
        menuMap = actions.FindActionMap(menuMapName, false);

        pauseAction = playerMap?.FindAction(pauseActionName, false);
        statsAction = playerMap?.FindAction(statsActionName, false);
        menuCancelAction = menuMap?.FindAction(menuCancelActionName, false);
        closeEquipmentAction = menuMap?.FindAction(closeEquipmentActionName, false);
    }

    private void Subscribe()
    {
        if (isSubscribed)
            return;

        if (pauseAction != null)
            pauseAction.performed += HandlePausePerformed;

        if (statsAction != null)
            statsAction.performed += HandleStatsPerformed;

        if (menuCancelAction != null)
            menuCancelAction.performed += HandleMenuCancelPerformed;

        if (closeEquipmentAction != null)
            closeEquipmentAction.performed += HandleCloseEquipmentPerformed;

        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
            return;

        if (pauseAction != null)
            pauseAction.performed -= HandlePausePerformed;

        if (statsAction != null)
            statsAction.performed -= HandleStatsPerformed;

        if (menuCancelAction != null)
            menuCancelAction.performed -= HandleMenuCancelPerformed;

        if (closeEquipmentAction != null)
            closeEquipmentAction.performed -= HandleCloseEquipmentPerformed;

        isSubscribed = false;
    }

    private void ApplyContext(InputContext context)
    {
        switch (context)
        {
            case InputContext.Gameplay:
                SetMapEnabled(playerMap, true);
                SetMapEnabled(menuMap, false);
                break;

            case InputContext.BootExplore:
                SetMapEnabled(playerMap, true);
                SetMapEnabled(menuMap, false);
                break;

            case InputContext.MenuOnly:
                SetMapEnabled(playerMap, false);
                SetMapEnabled(menuMap, true);
                break;

            case InputContext.Cutscene:
                SetMapEnabled(playerMap, false);
                SetMapEnabled(menuMap, false);
                break;
        }
    }

    private static void SetMapEnabled(InputActionMap map, bool enabled)
    {
        if (map == null)
            return;

        if (enabled && !map.enabled)
            map.Enable();
        else if (!enabled && map.enabled)
            map.Disable();
    }

    private void HandlePausePerformed(InputAction.CallbackContext _)
    {
        if (CurrentContext != InputContext.Gameplay)
            return;

        OnPausePressed?.Invoke();
    }

    private void HandleStatsPerformed(InputAction.CallbackContext _)
    {
        if (CurrentContext != InputContext.Gameplay)
            return;

        OnStatsPressed?.Invoke();
    }

    private void HandleMenuCancelPerformed(InputAction.CallbackContext _)
    {
        if (CurrentContext != InputContext.MenuOnly)
            return;

        OnMenuCancelPressed?.Invoke();
    }

    private void HandleCloseEquipmentPerformed(InputAction.CallbackContext _)
    {
        if (CurrentContext != InputContext.MenuOnly)
            return;

        OnCloseEquipmentPressed?.Invoke();
    }
}