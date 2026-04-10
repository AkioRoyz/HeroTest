using UnityEngine;

public class CursorVisibilityController : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private bool hideCursor = true;
    [SerializeField] private bool lockCursor = false;

    private void Awake()
    {
        ApplyCursorState();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        Cursor.visible = !hideCursor;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }
}