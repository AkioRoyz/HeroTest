using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public enum InputMode
    {
        Player,
        Dialogue,
        Menu,
        QuestJournal,
        PauseMenu
    }

    public enum DeviceGroupType
    {
        KeyboardMouse,
        Gamepad
    }

    private InputSystem_Actions inputActions;

    private InputActionMap playerActionMap;
    private InputActionMap dialogueActionMap;
    private InputActionMap menuActionMap;
    private InputActionMap questJournalActionMap;

    private InputAction moveAction;

    private InputAction playerAttackAction;
    private InputAction playerGuardAction;
    private InputAction playerSpecialAction;
    private InputAction playerToggleTargetingAction;
    private InputAction playerUseAction;
    private InputAction playerStatsAction;
    private InputAction playerQuestJournalAction;
    private InputAction playerPauseAction;
    private InputAction playerItem1Action;
    private InputAction playerItem2Action;
    private InputAction playerItem3Action;
    private InputAction playerItem4Action;
    private InputAction playerItem5Action;

    private InputAction dialogueUpAction;
    private InputAction dialogueDownAction;
    private InputAction dialogueSelectAction;

    private InputAction menuUpAction;
    private InputAction menuDownAction;
    private InputAction menuLeftAction;
    private InputAction menuRightAction;
    private InputAction menuSelectAction;
    private InputAction menuSecondaryAction;       // UnequipOrDelete
    private InputAction menuDeleteAction;          // optional legacy/special action
    private InputAction menuCloseAction;           // Escape
    private InputAction menuCloseEquipmentAction;  // I

    private InputAction questJournalUpAction;
    private InputAction questJournalDownAction;
    private InputAction questJournalSelectAction;
    private InputAction questJournalBackAction;
    private InputAction questJournalMainTabAction;
    private InputAction questJournalSideTabAction;
    private InputAction questJournalPinQuestAction;
    private InputAction questJournalCloseAction;

    private bool missingCombatActionsWarningShown;

    public event Action OnAttackStarted;
    public event Action OnAttack;
    public event Action OnAttackCanceled;

    public event Action OnGuardStarted;
    public event Action OnGuardCanceled;

    public event Action OnSpecialPerformed;
    public event Action OnToggleTargeting;

    public event Action OnUse;
    public event Action OnStats;
    public event Action OnQuestJournal;
    public event Action OnPauseToggle;

    public event Action<int> OnQuickSlotPressed;

    public event Action OnDialogueUp;
    public event Action OnDialogueDown;
    public event Action OnDialogueSelect;

    public event Action OnMenuUp;
    public event Action OnMenuDown;
    public event Action OnMenuLeft;
    public event Action OnMenuRight;
    public event Action OnMenuSelect;

    public event Action OnMenuUnequipOrDelete;

    public event Action OnMenuUnequip
    {
        add => OnMenuUnequipOrDelete += value;
        remove => OnMenuUnequipOrDelete -= value;
    }

    public event Action OnMenuDelete;
    public event Action OnMenuClose;
    public event Action OnMenuCloseEquipment;

    public event Action OnQuestJournalUp;
    public event Action OnQuestJournalDown;
    public event Action OnQuestJournalSelect;
    public event Action OnQuestJournalBack;
    public event Action OnQuestJournalMainTab;
    public event Action OnQuestJournalSideTab;
    public event Action OnQuestJournalPinQuest;
    public event Action OnQuestJournalClose;

    public event Action OnPauseMenuUp;
    public event Action OnPauseMenuDown;
    public event Action OnPauseMenuSelect;

    public event Action OnActiveDeviceGroupChanged;

    public Vector2 MoveVector { get; private set; }
    public InputMode CurrentMode { get; private set; }

    public DeviceGroupType CurrentDeviceGroup { get; private set; } = DeviceGroupType.KeyboardMouse;
    public string CurrentDeviceLayoutName { get; private set; } = "Keyboard";

    public bool HasGuardAction => playerGuardAction != null;
    public bool HasSpecialAction => playerSpecialAction != null;
    public bool HasToggleTargetingAction => playerToggleTargetingAction != null;
    public bool HasMenuDeleteAction => menuDeleteAction != null;

    private bool IsMenuLikeMode => CurrentMode == InputMode.Menu || CurrentMode == InputMode.PauseMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureInitialized();
        SwitchToPlayerMode();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        UnsubscribeFromInput();

        if (inputActions != null)
        {
            inputActions.Dispose();
            inputActions = null;
        }

        Instance = null;
    }

    private void Update()
    {
        if (moveAction == null)
        {
            MoveVector = Vector2.zero;
            return;
        }

        if (CurrentMode == InputMode.Player)
        {
            MoveVector = moveAction.ReadValue<Vector2>().normalized;
        }
        else
        {
            MoveVector = Vector2.zero;
        }
    }

    private void EnsureInitialized()
    {
        if (inputActions != null)
            return;

        inputActions = new InputSystem_Actions();
        CacheActionReferences();
        SubscribeToInput();
    }

    private void CacheActionReferences()
    {
        if (inputActions == null || inputActions.asset == null)
            return;

        playerActionMap = inputActions.asset.FindActionMap("Player", false);
        dialogueActionMap = inputActions.asset.FindActionMap("Dialogue", false);
        menuActionMap = inputActions.asset.FindActionMap("Menu", false);
        questJournalActionMap = inputActions.asset.FindActionMap("QuestJournal", false);

        moveAction = FindRequiredAction(playerActionMap, "Player", "Moving");

        playerAttackAction = FindRequiredAction(playerActionMap, "Player", "Attack");
        playerGuardAction = FindOptionalAction(playerActionMap, "Guard");
        playerSpecialAction = FindOptionalAction(playerActionMap, "Special");
        playerToggleTargetingAction = FindOptionalAction(playerActionMap, "ToggleTargeting");
        playerUseAction = FindRequiredAction(playerActionMap, "Player", "Use");
        playerStatsAction = FindRequiredAction(playerActionMap, "Player", "Stats");
        playerQuestJournalAction = FindRequiredAction(playerActionMap, "Player", "QuestJournal");
        playerPauseAction = FindRequiredAction(playerActionMap, "Player", "Pause");
        playerItem1Action = FindRequiredAction(playerActionMap, "Player", "Item1");
        playerItem2Action = FindRequiredAction(playerActionMap, "Player", "Item2");
        playerItem3Action = FindRequiredAction(playerActionMap, "Player", "Item3");
        playerItem4Action = FindRequiredAction(playerActionMap, "Player", "Item4");
        playerItem5Action = FindRequiredAction(playerActionMap, "Player", "Item5");

        dialogueUpAction = FindRequiredAction(dialogueActionMap, "Dialogue", "UpSelect");
        dialogueDownAction = FindRequiredAction(dialogueActionMap, "Dialogue", "DownSelect");
        dialogueSelectAction = FindRequiredAction(dialogueActionMap, "Dialogue", "SelectChoise");

        menuUpAction = FindRequiredAction(menuActionMap, "Menu", "UpSelect");
        menuDownAction = FindRequiredAction(menuActionMap, "Menu", "DownSelect");
        menuLeftAction = FindRequiredAction(menuActionMap, "Menu", "LeftSelect");
        menuRightAction = FindRequiredAction(menuActionMap, "Menu", "RightSelect");
        menuSelectAction = FindRequiredAction(menuActionMap, "Menu", "SelectChoise");

        menuSecondaryAction =
            FindOptionalAction(menuActionMap, "UnequipOrDelete") ??
            FindOptionalAction(menuActionMap, "UnequipItem");

        menuDeleteAction = FindOptionalAction(menuActionMap, "Delete");
        menuCloseAction = FindRequiredAction(menuActionMap, "Menu", "CloseUI");
        menuCloseEquipmentAction = FindRequiredAction(menuActionMap, "Menu", "CloseEquipment");

        questJournalUpAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "UpSelect");
        questJournalDownAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "DownSelect");
        questJournalSelectAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "Select");
        questJournalBackAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "Back");
        questJournalMainTabAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "MainTab");
        questJournalSideTabAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "SideTab");
        questJournalPinQuestAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "PinQuest");
        questJournalCloseAction = FindRequiredAction(questJournalActionMap, "QuestJournal", "CloseUI");

        ShowMissingCombatActionWarningOnce();
    }

    private InputAction FindRequiredAction(InputActionMap map, string mapName, string actionName)
    {
        if (map == null)
        {
            Debug.LogWarning($"GameInput: action map '{mapName}' was not found in InputSystem_Actions.");
            return null;
        }

        InputAction action = map.FindAction(actionName, false);

        if (action == null)
        {
            Debug.LogWarning($"GameInput: action '{mapName}/{actionName}' was not found in InputSystem_Actions.");
        }

        return action;
    }

    private InputAction FindOptionalAction(InputActionMap map, string actionName)
    {
        if (map == null)
            return null;

        return map.FindAction(actionName, false);
    }

    private void ShowMissingCombatActionWarningOnce()
    {
        if (missingCombatActionsWarningShown)
            return;

        List<string> missingActions = new();

        if (playerGuardAction == null) missingActions.Add("Player/Guard");
        if (playerSpecialAction == null) missingActions.Add("Player/Special");
        if (playerToggleTargetingAction == null) missingActions.Add("Player/ToggleTargeting");

        if (missingActions.Count > 0)
        {
            Debug.LogWarning(
                "GameInput: optional combat actions are missing from InputSystem_Actions: " +
                string.Join(", ", missingActions) +
                ". Add them in the Player action map to enable guard, special and targeting toggle.");
        }

        missingCombatActionsWarningShown = true;
    }

    public string GetCurrentBindingGroupName()
    {
        return CurrentDeviceGroup == DeviceGroupType.Gamepad ? "Gamepad" : "KeyboardMouse";
    }

    public string GetCurrentDeviceLayoutName()
    {
        return CurrentDeviceLayoutName;
    }

    public InputAction GetMenuAction_Select()
    {
        EnsureInitialized();
        return menuSelectAction;
    }

    public InputAction GetMenuAction_UnequipOrDelete()
    {
        EnsureInitialized();
        return menuSecondaryAction;
    }

    public InputAction GetMenuAction_Unequip()
    {
        EnsureInitialized();
        return menuSecondaryAction;
    }

    public InputAction GetMenuAction_Delete()
    {
        EnsureInitialized();
        return menuDeleteAction;
    }

    public bool IsAttackPressed()
    {
        return playerAttackAction != null && playerAttackAction.IsPressed();
    }

    public bool IsGuardPressed()
    {
        return playerGuardAction != null && playerGuardAction.IsPressed();
    }

    private void SubscribeToInput()
    {
        SubscribeStarted(playerAttackAction, OnAttackStartedPerformed);
        SubscribePerformed(playerAttackAction, OnAttackPerformed);
        SubscribeCanceled(playerAttackAction, OnAttackCanceledPerformed);

        SubscribeStarted(playerGuardAction, OnGuardStartedPerformed);
        SubscribeCanceled(playerGuardAction, OnGuardCanceledPerformed);

        SubscribePerformed(playerSpecialAction, OnSpecialPerformedInternal);
        SubscribePerformed(playerToggleTargetingAction, OnToggleTargetingPerformedInternal);

        SubscribePerformed(playerUseAction, OnUsePerformed);
        SubscribePerformed(playerStatsAction, OnStatsPerformed);
        SubscribePerformed(playerQuestJournalAction, OnQuestJournalPerformed);
        SubscribePerformed(playerPauseAction, OnPausePerformed);
        SubscribePerformed(playerItem1Action, OnQuickSlotPerformed);
        SubscribePerformed(playerItem2Action, OnQuickSlotPerformed);
        SubscribePerformed(playerItem3Action, OnQuickSlotPerformed);
        SubscribePerformed(playerItem4Action, OnQuickSlotPerformed);
        SubscribePerformed(playerItem5Action, OnQuickSlotPerformed);

        SubscribePerformed(dialogueUpAction, OnDialogueUpPerformed);
        SubscribePerformed(dialogueDownAction, OnDialogueDownPerformed);
        SubscribePerformed(dialogueSelectAction, OnDialogueSelectPerformed);

        SubscribePerformed(menuUpAction, OnMenuUpPerformed);
        SubscribePerformed(menuDownAction, OnMenuDownPerformed);
        SubscribePerformed(menuLeftAction, OnMenuLeftPerformed);
        SubscribePerformed(menuRightAction, OnMenuRightPerformed);
        SubscribePerformed(menuSelectAction, OnMenuSelectPerformed);
        SubscribePerformed(menuSecondaryAction, OnMenuSecondaryPerformed);
        SubscribePerformed(menuDeleteAction, OnMenuDeletePerformed);
        SubscribePerformed(menuCloseAction, OnMenuClosePerformed);
        SubscribePerformed(menuCloseEquipmentAction, OnMenuCloseEquipmentPerformed);

        SubscribePerformed(questJournalUpAction, OnQuestJournalUpPerformed);
        SubscribePerformed(questJournalDownAction, OnQuestJournalDownPerformed);
        SubscribePerformed(questJournalSelectAction, OnQuestJournalSelectPerformed);
        SubscribePerformed(questJournalBackAction, OnQuestJournalBackPerformed);
        SubscribePerformed(questJournalMainTabAction, OnQuestJournalMainTabPerformed);
        SubscribePerformed(questJournalSideTabAction, OnQuestJournalSideTabPerformed);
        SubscribePerformed(questJournalPinQuestAction, OnQuestJournalPinQuestPerformed);
        SubscribePerformed(questJournalCloseAction, OnQuestJournalClosePerformed);
    }

    private void UnsubscribeFromInput()
    {
        UnsubscribeStarted(playerAttackAction, OnAttackStartedPerformed);
        UnsubscribePerformed(playerAttackAction, OnAttackPerformed);
        UnsubscribeCanceled(playerAttackAction, OnAttackCanceledPerformed);

        UnsubscribeStarted(playerGuardAction, OnGuardStartedPerformed);
        UnsubscribeCanceled(playerGuardAction, OnGuardCanceledPerformed);

        UnsubscribePerformed(playerSpecialAction, OnSpecialPerformedInternal);
        UnsubscribePerformed(playerToggleTargetingAction, OnToggleTargetingPerformedInternal);

        UnsubscribePerformed(playerUseAction, OnUsePerformed);
        UnsubscribePerformed(playerStatsAction, OnStatsPerformed);
        UnsubscribePerformed(playerQuestJournalAction, OnQuestJournalPerformed);
        UnsubscribePerformed(playerPauseAction, OnPausePerformed);
        UnsubscribePerformed(playerItem1Action, OnQuickSlotPerformed);
        UnsubscribePerformed(playerItem2Action, OnQuickSlotPerformed);
        UnsubscribePerformed(playerItem3Action, OnQuickSlotPerformed);
        UnsubscribePerformed(playerItem4Action, OnQuickSlotPerformed);
        UnsubscribePerformed(playerItem5Action, OnQuickSlotPerformed);

        UnsubscribePerformed(dialogueUpAction, OnDialogueUpPerformed);
        UnsubscribePerformed(dialogueDownAction, OnDialogueDownPerformed);
        UnsubscribePerformed(dialogueSelectAction, OnDialogueSelectPerformed);

        UnsubscribePerformed(menuUpAction, OnMenuUpPerformed);
        UnsubscribePerformed(menuDownAction, OnMenuDownPerformed);
        UnsubscribePerformed(menuLeftAction, OnMenuLeftPerformed);
        UnsubscribePerformed(menuRightAction, OnMenuRightPerformed);
        UnsubscribePerformed(menuSelectAction, OnMenuSelectPerformed);
        UnsubscribePerformed(menuSecondaryAction, OnMenuSecondaryPerformed);
        UnsubscribePerformed(menuDeleteAction, OnMenuDeletePerformed);
        UnsubscribePerformed(menuCloseAction, OnMenuClosePerformed);
        UnsubscribePerformed(menuCloseEquipmentAction, OnMenuCloseEquipmentPerformed);

        UnsubscribePerformed(questJournalUpAction, OnQuestJournalUpPerformed);
        UnsubscribePerformed(questJournalDownAction, OnQuestJournalDownPerformed);
        UnsubscribePerformed(questJournalSelectAction, OnQuestJournalSelectPerformed);
        UnsubscribePerformed(questJournalBackAction, OnQuestJournalBackPerformed);
        UnsubscribePerformed(questJournalMainTabAction, OnQuestJournalMainTabPerformed);
        UnsubscribePerformed(questJournalSideTabAction, OnQuestJournalSideTabPerformed);
        UnsubscribePerformed(questJournalPinQuestAction, OnQuestJournalPinQuestPerformed);
        UnsubscribePerformed(questJournalCloseAction, OnQuestJournalClosePerformed);
    }

    private static void SubscribeStarted(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null) action.started += callback;
    }

    private static void SubscribePerformed(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null) action.performed += callback;
    }

    private static void SubscribeCanceled(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null) action.canceled += callback;
    }

    private static void UnsubscribeStarted(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null) action.started -= callback;
    }

    private static void UnsubscribePerformed(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null) action.performed -= callback;
    }

    private static void UnsubscribeCanceled(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null) action.canceled -= callback;
    }

    public void SwitchToPlayerMode()
    {
        EnableOnly(playerActionMap, InputMode.Player);
    }

    public void SwitchToDialogueMode()
    {
        EnableOnly(dialogueActionMap, InputMode.Dialogue);
    }

    public void SwitchToMenuMode()
    {
        EnableOnly(menuActionMap, InputMode.Menu);
    }

    public void SwitchToQuestJournalMode()
    {
        EnableOnly(questJournalActionMap, InputMode.QuestJournal);
    }

    public void SwitchToPauseMenuMode()
    {
        EnableOnly(menuActionMap, InputMode.PauseMenu);
    }

    private void EnableOnly(InputActionMap mapToEnable, InputMode newMode)
    {
        EnsureInitialized();

        playerActionMap?.Disable();
        dialogueActionMap?.Disable();
        menuActionMap?.Disable();
        questJournalActionMap?.Disable();

        mapToEnable?.Enable();

        MoveVector = Vector2.zero;
        CurrentMode = newMode;
    }

    private void UpdateDeviceGroup(InputAction.CallbackContext context)
    {
        if (context.control == null || context.control.device == null)
            return;

        InputDevice device = context.control.device;

        DeviceGroupType newGroup =
            (device is Keyboard || device is Mouse)
                ? DeviceGroupType.KeyboardMouse
                : DeviceGroupType.Gamepad;

        bool groupChanged = newGroup != CurrentDeviceGroup;
        bool layoutChanged = CurrentDeviceLayoutName != device.layout;

        CurrentDeviceGroup = newGroup;
        CurrentDeviceLayoutName = device.layout;

        if (groupChanged || layoutChanged)
        {
            OnActiveDeviceGroupChanged?.Invoke();
        }
    }

    private void OnAttackStartedPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnAttackStarted?.Invoke();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnAttack?.Invoke();
    }

    private void OnAttackCanceledPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnAttackCanceled?.Invoke();
    }

    private void OnGuardStartedPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnGuardStarted?.Invoke();
    }

    private void OnGuardCanceledPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnGuardCanceled?.Invoke();
    }

    private void OnSpecialPerformedInternal(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnSpecialPerformed?.Invoke();
    }

    private void OnToggleTargetingPerformedInternal(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnToggleTargeting?.Invoke();
    }

    private void OnUsePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnUse?.Invoke();
    }

    private void OnStatsPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnStats?.Invoke();
    }

    private void OnQuestJournalPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuestJournal?.Invoke();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnPauseToggle?.Invoke();
    }

    private void OnQuickSlotPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;

        UpdateDeviceGroup(context);

        int slotNumber = GetQuickSlotNumber(context.action);
        if (slotNumber <= 0) return;

        OnQuickSlotPressed?.Invoke(slotNumber);
    }

    private int GetQuickSlotNumber(InputAction action)
    {
        if (action == null)
            return -1;

        if (action == playerItem1Action) return 1;
        if (action == playerItem2Action) return 2;
        if (action == playerItem3Action) return 3;
        if (action == playerItem4Action) return 4;
        if (action == playerItem5Action) return 5;

        return -1;
    }

    private void OnDialogueUpPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        UpdateDeviceGroup(context);
        OnDialogueUp?.Invoke();
    }

    private void OnDialogueDownPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        UpdateDeviceGroup(context);
        OnDialogueDown?.Invoke();
    }

    private void OnDialogueSelectPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        UpdateDeviceGroup(context);
        OnDialogueSelect?.Invoke();
    }

    private void OnMenuUpPerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuUp?.Invoke();

        if (CurrentMode == InputMode.PauseMenu)
            OnPauseMenuUp?.Invoke();
    }

    private void OnMenuDownPerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuDown?.Invoke();

        if (CurrentMode == InputMode.PauseMenu)
            OnPauseMenuDown?.Invoke();
    }

    private void OnMenuLeftPerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuLeft?.Invoke();
    }

    private void OnMenuRightPerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuRight?.Invoke();
    }

    private void OnMenuSelectPerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuSelect?.Invoke();

        if (CurrentMode == InputMode.PauseMenu)
            OnPauseMenuSelect?.Invoke();
    }

    private void OnMenuSecondaryPerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuUnequipOrDelete?.Invoke();
    }

    private void OnMenuDeletePerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuDelete?.Invoke();
    }

    private void OnMenuClosePerformed(InputAction.CallbackContext context)
    {
        if (!IsMenuLikeMode) return;
        UpdateDeviceGroup(context);
        OnMenuClose?.Invoke();

        if (CurrentMode == InputMode.PauseMenu)
            OnPauseToggle?.Invoke();
    }

    private void OnMenuCloseEquipmentPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuCloseEquipment?.Invoke();
    }

    private void OnQuestJournalUpPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalUp?.Invoke();
    }

    private void OnQuestJournalDownPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalDown?.Invoke();
    }

    private void OnQuestJournalSelectPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalSelect?.Invoke();
    }

    private void OnQuestJournalBackPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalBack?.Invoke();
    }

    private void OnQuestJournalMainTabPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalMainTab?.Invoke();
    }

    private void OnQuestJournalSideTabPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalSideTab?.Invoke();
    }

    private void OnQuestJournalPinQuestPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalPinQuest?.Invoke();
    }

    private void OnQuestJournalClosePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.QuestJournal) return;
        UpdateDeviceGroup(context);
        OnQuestJournalClose?.Invoke();
    }
}