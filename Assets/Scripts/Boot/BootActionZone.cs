using UnityEngine;

public sealed class BootActionZone : MonoBehaviour
{
    public enum BootActionType
    {
        StartNewGame,
        OpenLoadMenu,
        ReturnToMainMenu,
        ExitGame
    }

    [SerializeField] private BootLocationController controller;
    [SerializeField] private BootActionType actionType;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot;
    private bool wasUsed;

    private void Reset()
    {
        if (controller == null)
            controller = FindFirstObjectByType<BootLocationController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (oneShot && wasUsed)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (controller == null)
        {
            Debug.LogError("[BootActionZone] BootLocationController is missing.", this);
            return;
        }

        wasUsed = true;

        switch (actionType)
        {
            case BootActionType.StartNewGame:
                controller.StartNewGame();
                break;

            case BootActionType.OpenLoadMenu:
                controller.OpenLoadMenu();
                break;

            case BootActionType.ReturnToMainMenu:
                controller.ReturnToMainMenu();
                break;

            case BootActionType.ExitGame:
                controller.ExitGame();
                break;
        }
    }
}