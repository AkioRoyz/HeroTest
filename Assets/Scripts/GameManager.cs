using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log($"[GameManager Awake] {name}, scene = {gameObject.scene.name}", gameObject);

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameManager was destroyed.", gameObject);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (Instance != this)
            return;

        DontDestroyOnLoad(gameObject);

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}