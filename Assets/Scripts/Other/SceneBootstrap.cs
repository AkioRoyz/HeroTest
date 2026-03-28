using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnOnlyIfMissing = true;
    [SerializeField] private bool moveExistingPlayerToSpawn = true;
    [SerializeField] private bool resetPlayerVelocityOnTeleport = true;

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    private void Start()
    {
        EnsurePlayerExistsAndPlaced();
    }

    private void EnsurePlayerExistsAndPlaced()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject existingPlayer = players.Length > 0 ? players[0] : null;

        if (players.Length > 1)
        {
            Debug.LogError($"[SceneBootstrap] Found {players.Length} objects with Player tag. There must be exactly one player.", this);
        }

        if (spawnOnlyIfMissing && existingPlayer != null)
        {
            if (moveExistingPlayerToSpawn && spawnPoint != null)
            {
                existingPlayer.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                ResetPlayerPhysics(existingPlayer);

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

    private void ResetPlayerPhysics(GameObject player)
    {
        if (!resetPlayerVelocityOnTeleport || player == null)
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}