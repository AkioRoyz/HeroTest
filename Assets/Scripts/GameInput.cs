using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public enum InputMode
    {
        Player,
        Dialogue,
        Menu
    }

    private InputSystem_Actions inputActions;

    public event Action OnAttack;
    public event Action OnUse;
    public event Action OnStats;
    public event Action<int> OnQuickSlotPressed;

    public event Action OnDialogueUp;
    public event Action OnDialogueDown;
    public event Action OnDialogueSelect;
    public event Action OnDialogueClose;

    public event Action OnMenuUp;
    public event Action OnMenuDown;
    public event Action OnMenuSelect;
    public event Action OnMenuClose;

    public Vector2 MoveVector { get; private set; }
    public InputMode CurrentMode { get; private set; }

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

    private void SubscribeToInput()
    {
        // Player
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Use.performed += OnUsePerformed;
        inputActions.Player.Stats.performed += OnStatsPerformed;

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
        inputActions.Menu.SelectChoise.performed += OnMenuSelectPerformed;
        inputActions.Menu.CloseUI.performed += OnMenuClosePerformed;
    }

    private void UnsubscribeFromInput()
    {
        // Player
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Use.performed -= OnUsePerformed;
        inputActions.Player.Stats.performed -= OnStatsPerformed;

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
        inputActions.Menu.SelectChoise.performed -= OnMenuSelectPerformed;
        inputActions.Menu.CloseUI.performed -= OnMenuClosePerformed;
    }

    public void SwitchToPlayerMode()
    {
        inputActions.Player.Enable();
        inputActions.Dialogue.Disable();
        inputActions.Menu.Disable();

        CurrentMode = InputMode.Player;
    }

    public void SwitchToDialogueMode()
    {
        inputActions.Player.Disable();
        inputActions.Dialogue.Enable();
        inputActions.Menu.Disable();

        MoveVector = Vector2.zero;
        CurrentMode = InputMode.Dialogue;
    }

    public void SwitchToMenuMode()
    {
        inputActions.Player.Disable();
        inputActions.Dialogue.Disable();
        inputActions.Menu.Enable();

        MoveVector = Vector2.zero;
        CurrentMode = InputMode.Menu;
    }

    // -------------------- Player --------------------

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnAttack?.Invoke();
    }

    private void OnUsePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnUse?.Invoke();
    }

    private void OnStatsPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player && CurrentMode != InputMode.Menu) return;
        OnStats?.Invoke();
    }

    private void OnItem1Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnQuickSlotPressed?.Invoke(1);
    }

    private void OnItem2Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnQuickSlotPressed?.Invoke(2);
    }

    private void OnItem3Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnQuickSlotPressed?.Invoke(3);
    }

    private void OnItem4Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnQuickSlotPressed?.Invoke(4);
    }

    private void OnItem5Performed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Player) return;
        OnQuickSlotPressed?.Invoke(5);
    }

    // -------------------- Dialogue --------------------

    private void OnDialogueUpPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        OnDialogueUp?.Invoke();
    }

    private void OnDialogueDownPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        OnDialogueDown?.Invoke();
    }

    private void OnDialogueSelectPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        OnDialogueSelect?.Invoke();
    }

    private void OnDialogueClosePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Dialogue) return;
        OnDialogueClose?.Invoke();
    }

    // -------------------- Menu --------------------

    private void OnMenuUpPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        OnMenuUp?.Invoke();
    }

    private void OnMenuDownPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        OnMenuDown?.Invoke();
    }

    private void OnMenuSelectPerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        OnMenuSelect?.Invoke();
    }

    private void OnMenuClosePerformed(InputAction.CallbackContext context)
    {
        if (CurrentMode != InputMode.Menu) return;
        OnMenuClose?.Invoke();
    }
}