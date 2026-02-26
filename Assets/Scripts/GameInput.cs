using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] private InputSystem_Actions inputActions;
    public event Action OnAttack;
    public event Action OnUse;
    public Vector2 moveVector {  get; private set; }

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }

    private void Update()
    {
        moveVector = inputActions.Player.Moving.ReadValue<Vector2>();
        moveVector = moveVector.normalized;
    }

    private void OnEnable()
    {
        inputActions.Player.Attack.performed += AttackAction;
        inputActions.Player.Use.performed += UsePlayer;
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= AttackAction;
        inputActions.Player.Use.performed -= UsePlayer;
    }

    private void AttackAction(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke();
    }

    private void UsePlayer(InputAction.CallbackContext context)
    {
        OnUse?.Invoke(); 
    }
}
