using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnOnlyIfMissing = true;
    [SerializeField] private bool moveExistingPlayerToSpawn = true;

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    private void Start()
    {
        EnsurePlayerExistsAndPlaced();
    }

    private void EnsurePlayerExistsAndPlaced()
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (spawnOnlyIfMissing && existingPlayer != null)
        {
            if (moveExistingPlayerToSpawn && spawnPoint != null)
            {
                existingPlayer.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

                if (showLogs)
                    Debug.Log($"[SceneBootstrap] Existing player moved to spawn: {existingPlayer.name}", this);
            }
            else if (showLogs)
            {
                Debug.Log($"[SceneBootstrap] Player already exists: {existingPlayer.name}", this);
            }

            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogWarning("[SceneBootstrap] playerPrefab is not assigned.", this);
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);

        if (showLogs)
            Debug.Log($"[SceneBootstrap] Spawned player: {player.name}", this);
    }
}