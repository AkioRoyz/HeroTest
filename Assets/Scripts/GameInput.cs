using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public enum InputMode
    {
        Player,
        Dialogue,
        Menu,
        QuestJournal
    }

    public enum DeviceGroupType
    {
        KeyboardMouse,
        Gamepad
    }

    private InputSystem_Actions inputActions;

    public event Action OnAttack;
    public event Action OnUse;
    public event Action OnStats;
    public event Action OnQuestJournal;
    public event Action<int> OnQuickSlotPressed;

    public event Action OnDialogueUp;
    public event Action OnDialogueDown;
    public event Action OnDialogueSelect;
    public event Action OnDialogueClose;

    public event Action OnMenuUp;
    public event Action OnMenuDown;
    public event Action OnMenuLeft;
    public event Action OnMenuRight;
    public event Action OnMenuSelect;
    public event Action OnMenuUnequip;
    public event Action OnMenuClose;

    public event Action OnQuestJournalUp;
    public event Action OnQuestJournalDown;
    public event Action OnQuestJournalSelect;
    public event Action OnQuestJournalBack;
    public event Action OnQuestJournalMainTab;
    public event Action OnQuestJournalSideTab;
    public event Action OnQuestJournalPinQuest;
    public event Action OnQuestJournalClose;

    public event Action OnActiveDeviceGroupChanged;

    public Vector2 MoveVector { get; private set; }
    public InputMode CurrentMode { get; private set; }

    public DeviceGroupType CurrentDeviceGroup { get; private set; } = DeviceGroupType.KeyboardMouse;
    public string CurrentDeviceLayoutName { get; private set; } = "Keyboard";

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        SubscribeToInput();
        SwitchToPlayerMode();
    }

    private void OnDestroy()
    {
        UnsubscribeFromInput();
        inputActions.Dispose();
    }

    private void Update()
    {
        if (CurrentMode == InputMode.Player)
        {
            MoveVector = inputActions.Player.Moving.ReadValue<Vector2>().normalized;
        }
        else
        {
            MoveVector = Vector2.zero;
        }
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
        return inputActions.Menu.SelectChoise;
    }

    public InputAction GetMenuAction_Unequip()
    {
        return inputActions.Menu.UnequipItem;
    }

    private void SubscribeToInput()
    {
        // Player
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Use.performed += OnUsePerformed;
        inputActions.Player.Stats.performed += OnStatsPerformed;
        inputActions.Player.QuestJournal.performed += OnQuestJournalPerformed;

        inputActions.Player.Item1.performed += OnItem1Performed;
        inputActions.Player.Item2.performed += OnItem2Performed;
        inputActions.Player.Item3.performed += OnItem3Performed;
        inputActions.Player.Item4.performed += OnItem4Performed;
        inputActions.Player.Item5.performed += OnItem5Performed;

        // Dialogue
        inputActions.Dialogue.UpSelect.performed += OnDialogueUpPerformed;
        inputActions.Dialogue.DownSelect.performed += OnDialogueDownPerformed;
        inputActions.Dialogue.SelectChoise.performed += OnDialogueSelectPerformed;
        inputActions.Dialogue.CloseUI.performed += OnDialogueClosePerformed;

        // Menu
        inputActions.Menu.UpSelect.performed += OnMenuUpPerformed;
        inputActions.Menu.DownSelect.performed += OnMenuDownPerformed;
        inputActions.Menu.LeftSelect.performed += OnMenuLeftPerformed;
        inputActions.Menu.RightSelect.performed += OnMenuRightPerformed;
        inputActions.Menu.SelectChoise.performed += OnMenuSelectPerformed;
        inputActions.Menu.UnequipItem.performed += OnMenuUnequipPerformed;
        inputActions.Menu.CloseUI.performed += OnMenuClosePerformed;

        // QuestJournal
        inputActions.QuestJournal.UpSelect.performed += OnQuestJournalUpPerformed;
        inputActions.QuestJournal.DownSelect.performed += OnQuestJournalDownPerformed;
        inputActions.QuestJournal.Select.performed += OnQuestJournalSelectPerformed;
        inputActions.QuestJournal.Back.performed += OnQuestJournalBackPerformed;
        inputActions.QuestJournal.MainTab.performed += OnQuestJournalMainTabPerformed;
        inputActions.QuestJournal.SideTab.performed += OnQuestJournalSideTabPerformed;
        inputActions.QuestJournal.PinQuest.performed += OnQuestJournalPinQuestPerformed;
        inputActions.QuestJournal.CloseUI.performed += OnQuestJournalClosePerformed;
    }

    private void UnsubscribeFromInput()
    {
        // Player
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Use.performed -= OnUsePerformed;
        inputActions.Player.Stats.performed -= OnStatsPerformed;
        inputActions.Player.QuestJournal.performed -= OnQuestJournalPerformed;

        inputActions.Player.Item1.performed -= OnItem1Performed;
        inputActions.Player.Item2.performed -= OnItem2Performed;
        inputActions.Player.Item3.performed -= OnItem3Performed;
        inputActions.Player.Item4.performed -= OnItem4Performed;
        inputActions.Player.Item5.performed -= OnItem5Performed;

        // Dialogue
        inputActions.Dialogue.UpSelect.performed -= OnDialogueUpPerformed;
        inputActions.Dialogue.DownSelect.performed -= OnDialogueDownPerformed;
        inputActions.Dialogue.SelectChoise.performed -= OnDialogueSelectPerformed;
        inputActions.Dialogue.CloseUI.performed -= OnDialogueClosePerformed;

        // Menu
        inputActions.Menu.UpSelect.performed -= OnMenuUpPerformed;
        inputActions.Menu.DownSelect.performed -= OnMenuDownPerformed;
        inputActions.Menu.LeftSelect.performed -= OnMenuLeftPerformed;
        inputActions.Menu.RightSelect.performed -= OnMenuRightPerformed;
        inputActions.Menu.SelectChoise.performed -= OnMenuSelectPerformed;
        inputActions.Menu.UnequipItem.performed -= OnMenuUnequipPerformed;
        inputActions.Menu.CloseUI.performed -= OnMenuClosePerformed;

        // QuestJournal
        inputActions.QuestJournal.UpSelect.performed -= OnQuestJournalUpPerformed;
        inputActions.QuestJournal.DownSelect.performed -= OnQuestJournalDownPerformed;
        inputActions.QuestJournal.Select.performed -= OnQuestJournalSelectPerformed;
        inputActions.QuestJournal.Back.performed -= OnQuestJournalBackPerformed;
        inputActions.QuestJournal.MainTab.performed -= OnQuestJournalMainTabPerformed;
        inputActions.QuestJournal.SideTab.performed -= OnQuestJournalSideTabPerformed;
        inputActions.QuestJournal.PinQuest.performed -= OnQuestJournalPinQuestPerformed;
        inputActions.QuestJournal.CloseUI.performed -= OnQuestJournalClosePerformed;
    }

    public void SwitchToPlayerMode()
    {
        inputActions.Player.Enable();
        inputActions.Dialogue.Disable();
        inputActions.Menu.Disable();
        inputActions.QuestJournal.Disable();

        MoveVector = Vector2.zero;
        CurrentMode = InputMode.Player;
    }

    public void SwitchToDialogueMode()
    {
        inputActions.Player.Disable();
        inputActions.Dialogue.Enable();
        inputActions.Menu.Disable();
        inputActions.QuestJournal.Disable();

        MoveVector = Vector2.zero;
        CurrentMode = InputMode.Dialogue;
    }

    public void SwitchToMenuMode()
    {
        inputActions.Player.Disable();
        inputActions.Dialogue.Disable();
        inputActions.Menu.Enable();
        inputActions.QuestJournal.Disable();

        MoveVector = Vector2.zero;
        CurrentMode = InputMode.Menu;
    }

    public void SwitchToQuestJournalMode()
    {
        inputActions.Player.Disable();
        inputActions.Dialogue.Disable();
        inputActions.Menu.Disable();
        inputActions.QuestJournal.Enable();

        MoveVector = Vector2.zero;
        CurrentMode = InputMode.QuestJournal;
    }

    private void UpdateDeviceGroup(InputAction.CallbackContext context)
    {
        if (context.control == null || context.control.device == null)
            return;

        var device = context.control.device;
        DeviceGroupType newGroup;

        if (device is Keyboard || device is Mouse)
        {
            newGroup = DeviceGroupType.KeyboardMouse;
        }
        else
        {
            newGroup = DeviceGroupType.Gamepad;
        }

        bool groupChanged = newGroup != CurrentDeviceGroup;
        bool layoutChanged = CurrentDeviceLayoutName != device.layout;

        CurrentDeviceGroup = newGroup;
        CurrentDeviceLayoutName = device.layout;

        if (groupChanged || layoutChanged)
        {
            OnActiveDeviceGroupChanged?.Invoke();
        }

        Debug.Log($"Active input device: {device.displayName}, layout: {device.layout}, group: {CurrentDeviceGroup}");
    }

    // -------------------- Player --------------------

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnAttack?.Invoke();
    }

    private void OnUsePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnUse?.Invoke();
    }

    private void OnStatsPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player && CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnStats?.Invoke();
    }

    private void OnQuestJournalPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuestJournal?.Invoke();
    }

    private void OnItem1Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuickSlotPressed?.Invoke(1);
    }

    private void OnItem2Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuickSlotPressed?.Invoke(2);
    }

    private void OnItem3Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuickSlotPressed?.Invoke(3);
    }

    private void OnItem4Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuickSlotPressed?.Invoke(4);
    }

    private void OnItem5Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        UpdateDeviceGroup(context);
        OnQuickSlotPressed?.Invoke(5);
    }

    // -------------------- Dialogue --------------------

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

    private void OnDialogueClosePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        UpdateDeviceGroup(context);
        OnDialogueClose?.Invoke();
    }

    // -------------------- Menu --------------------

    private void OnMenuUpPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuUp?.Invoke();
    }

    private void OnMenuDownPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuDown?.Invoke();
    }

    private void OnMenuLeftPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuLeft?.Invoke();
    }

    private void OnMenuRightPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuRight?.Invoke();
    }

    private void OnMenuSelectPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuSelect?.Invoke();
    }

    private void OnMenuUnequipPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuUnequip?.Invoke();
    }

    private void OnMenuClosePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        UpdateDeviceGroup(context);
        OnMenuClose?.Invoke();
    }

    // -------------------- QuestJournal --------------------

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