using UnityEngine;

public sealed class PersistentServicesRoot : MonoBehaviour
{
    public static PersistentServicesRoot Instance { get; private set; }

    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}