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

        Transform resolvedSpawn = ResolveSpawnPoint();

        if (spawnOnlyIfMissing && existingPlayer != null)
        {
            if (moveExistingPlayerToSpawn && resolvedSpawn != null)
            {
                existingPlayer.transform.SetPositionAndRotation(resolvedSpawn.position, resolvedSpawn.rotation);
                ResetPlayerPhysics(existingPlayer);

                if (showLogs)
                    Debug.Log($"[SceneBootstrap] Existing player moved to spawn: {resolvedSpawn.name}", this);
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

        Vector3 spawnPos = resolvedSpawn != null ? resolvedSpawn.position : Vector3.zero;
        Quaternion spawnRot = resolvedSpawn != null ? resolvedSpawn.rotation : Quaternion.identity;

        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);

        if (showLogs)
            Debug.Log($"[SceneBootstrap] Spawned player: {player.name}", this);
    }

    private Transform ResolveSpawnPoint()
    {
        string pendingEntryPointId = SceneTransitionState.ConsumePendingEntryPoint();

        if (!string.IsNullOrWhiteSpace(pendingEntryPointId))
        {
            SceneEntryPoint[] entryPoints = FindObjectsByType<SceneEntryPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < entryPoints.Length; i++)
            {
                SceneEntryPoint entryPoint = entryPoints[i];
                if (entryPoint != null && entryPoint.EntryPointId == pendingEntryPointId)
                {
                    if (showLogs)
                        Debug.Log($"[SceneBootstrap] Entry point resolved: {pendingEntryPointId}", this);

                    return entryPoint.transform;
                }
            }

            Debug.LogWarning($"[SceneBootstrap] Entry point '{pendingEntryPointId}' not found. Fallback to default spawnPoint.", this);
        }

        return spawnPoint;
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