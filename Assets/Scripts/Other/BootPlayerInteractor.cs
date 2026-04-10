using UnityEngine;

public class BootPlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private BootLocationController bootLocationController;

    private GameInput subscribedInput;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RebindInput();
    }

    private void OnDisable()
    {
        UnbindInput();
    }

    private void ResolveReferences()
    {
        if (gameInput == null)
            gameInput = GameInput.Instance != null ? GameInput.Instance : FindFirstObjectByType<GameInput>();

        if (bootLocationController == null)
            bootLocationController = FindFirstObjectByType<BootLocationController>();
    }

    private void RebindInput()
    {
        UnbindInput();

        if (gameInput == null)
            return;

        gameInput.OnUse += HandleUse;
        subscribedInput = gameInput;
    }

    private void UnbindInput()
    {
        if (subscribedInput == null)
            return;

        subscribedInput.OnUse -= HandleUse;
        subscribedInput = null;
    }

    private void HandleUse()
    {
        if (bootLocationController == null)
            return;

        bootLocationController.TryInteract();
    }
}