using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BootActionTrigger : MonoBehaviour
{
    public enum BootActionType
    {
        StartNewGame,
        OpenLoadMenu,
        ExitGame
    }

    [Header("Action")]
    [SerializeField] private BootActionType actionType = BootActionType.StartNewGame;
    [SerializeField] private BootLocationController bootLocationController;

    [Header("Visuals")]
    [SerializeField] private GameObject idleVisual;
    [SerializeField] private GameObject focusedVisual;
    [SerializeField] private GameObject promptVisual;

    private BootPlayerInteractor currentInteractor;

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        SetFocused(false);
    }

    private void OnDisable()
    {
        if (bootLocationController != null)
            bootLocationController.UnregisterAvailableTrigger(this);

        currentInteractor = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BootPlayerInteractor interactor = other.GetComponentInParent<BootPlayerInteractor>();
        if (interactor == null)
            return;

        ResolveController();

        currentInteractor = interactor;
        bootLocationController?.RegisterAvailableTrigger(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BootPlayerInteractor interactor = other.GetComponentInParent<BootPlayerInteractor>();
        if (interactor == null)
            return;

        if (currentInteractor != interactor)
            return;

        bootLocationController?.UnregisterAvailableTrigger(this);
        currentInteractor = null;
    }

    public void Execute(BootLocationController controller)
    {
        BootLocationController targetController = controller != null ? controller : bootLocationController;
        if (targetController == null)
            return;

        switch (actionType)
        {
            case BootActionType.StartNewGame:
                targetController.StartNewGame();
                break;

            case BootActionType.OpenLoadMenu:
                targetController.OpenLoadMenu();
                break;

            case BootActionType.ExitGame:
                targetController.ExitGame();
                break;
        }
    }

    public void SetFocused(bool isFocused)
    {
        if (idleVisual != null)
            idleVisual.SetActive(!isFocused);

        if (focusedVisual != null)
            focusedVisual.SetActive(isFocused);

        if (promptVisual != null)
            promptVisual.SetActive(isFocused);
    }

    private void ResolveController()
    {
        if (bootLocationController == null)
            bootLocationController = FindFirstObjectByType<BootLocationController>();
    }
}