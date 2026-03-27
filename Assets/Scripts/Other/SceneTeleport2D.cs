using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneTeleport2D : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string targetSceneName;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    private bool isLoading;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (showLogs)
            Debug.Log($"[Teleport] В триггер вошёл: {other.name}", this);

        // Если collider висит на дочернем объекте, берём объект Rigidbody2D
        GameObject enteredObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        if (showLogs)
            Debug.Log($"[Teleport] Проверяем объект: {enteredObject.name}, tag = {enteredObject.tag}", this);

        if (!enteredObject.CompareTag(playerTag))
        {
            if (showLogs)
                Debug.Log($"[Teleport] Это не игрок. Ожидался tag: {playerTag}", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("[Teleport] Не указано имя сцены.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError($"[Teleport] Сцена '{targetSceneName}' не найдена в списке сцен билда.", this);
            return;
        }

        isLoading = true;

        if (showLogs)
            Debug.Log($"[Teleport] Загружаем сцену: {targetSceneName}", this);

        SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
    }
}