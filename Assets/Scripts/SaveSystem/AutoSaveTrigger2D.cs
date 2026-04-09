using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AutoSaveTrigger2D : MonoBehaviour
{
    [SerializeField] private bool oneShot = false;
    [SerializeField] private bool disableAfterSave = true;
    [SerializeField] private bool showLogs = true;

    private bool alreadyTriggered;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyTriggered && oneShot)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (SaveSystem.Instance == null)
        {
            Debug.LogWarning("[AutoSaveTrigger2D] SaveSystem.Instance is missing.", this);
            return;
        }

        bool success = SaveSystem.Instance.SaveToAutoSlot();
        if (!success)
            return;

        alreadyTriggered = true;

        if (showLogs)
            Debug.Log("[AutoSaveTrigger2D] Auto save created.", this);

        if (oneShot && disableAfterSave)
            gameObject.SetActive(false);
    }
}