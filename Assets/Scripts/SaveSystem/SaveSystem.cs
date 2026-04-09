using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public event Action OnSaveSlotsChanged;

    [Header("Config")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField][Min(1)] private int regularSlotCount = 3;
    [SerializeField] private bool showLogs = true;

    private GameSaveData pendingLoadData;
    private bool pendingInitializeNewGameAfterSceneLoad;
    private bool applyingPendingLoad;

    public int RegularSlotCount => regularSlotCount;
    public bool HasPendingLoad => pendingLoadData != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[SaveSystem] Duplicate instance destroyed: {name}", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureSaveDirectoryExists();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool HasAnySave()
    {
        if (HasAutoSave())
            return true;

        for (int i = 1; i <= regularSlotCount; i++)
        {
            if (HasRegularSave(i))
                return true;
        }

        return false;
    }

    public bool HasRegularSave(int slotIndex)
    {
        if (!IsRegularSlotIndexValid(slotIndex))
            return false;

        return File.Exists(GetRegularSlotPath(slotIndex));
    }

    public bool HasAutoSave()
    {
        return File.Exists(GetAutoSavePath());
    }

    public SaveSlotInfo GetRegularSlotInfo(int slotIndex)
    {
        SaveSlotInfo info = new SaveSlotInfo
        {
            Exists = false,
            IsAutoSlot = false,
            SlotIndex = slotIndex,
            SceneName = string.Empty,
            SavedAtText = string.Empty,
            PlayerLevel = 1,
            IsCorrupted = false
        };

        if (!IsRegularSlotIndexValid(slotIndex))
            return info;

        string path = GetRegularSlotPath(slotIndex);
        if (!File.Exists(path))
            return info;

        if (TryReadSave(path, out GameSaveData data))
        {
            FillInfoFromSaveData(info, data);
            info.Exists = true;
            return info;
        }

        info.Exists = true;
        info.IsCorrupted = true;
        return info;
    }

    public SaveSlotInfo GetAutoSlotInfo()
    {
        SaveSlotInfo info = new SaveSlotInfo
        {
            Exists = false,
            IsAutoSlot = true,
            SlotIndex = -1,
            SceneName = string.Empty,
            SavedAtText = string.Empty,
            PlayerLevel = 1,
            IsCorrupted = false
        };

        string path = GetAutoSavePath();
        if (!File.Exists(path))
            return info;

        if (TryReadSave(path, out GameSaveData data))
        {
            FillInfoFromSaveData(info, data);
            info.Exists = true;
            return info;
        }

        info.Exists = true;
        info.IsCorrupted = true;
        return info;
    }

    public bool SaveToRegularSlot(int slotIndex)
    {
        if (!IsRegularSlotIndexValid(slotIndex))
        {
            Debug.LogWarning($"[SaveSystem] Invalid regular slot index: {slotIndex}", this);
            return false;
        }

        GameSaveData data = CaptureCurrentGameData(false, slotIndex);
        if (data == null)
            return false;

        bool success = WriteSaveFile(GetRegularSlotPath(slotIndex), data);
        if (success)
            OnSaveSlotsChanged?.Invoke();

        return success;
    }

    public bool SaveToAutoSlot()
    {
        GameSaveData data = CaptureCurrentGameData(true, -1);
        if (data == null)
            return false;

        bool success = WriteSaveFile(GetAutoSavePath(), data);
        if (success)
            OnSaveSlotsChanged?.Invoke();

        return success;
    }

    public void LoadFromRegularSlot(int slotIndex)
    {
        if (!IsRegularSlotIndexValid(slotIndex))
        {
            Debug.LogWarning($"[SaveSystem] Invalid regular slot index: {slotIndex}", this);
            return;
        }

        string path = GetRegularSlotPath(slotIndex);
        if (!TryReadSave(path, out GameSaveData data))
            return;

        BeginLoad(data);
    }

    public void LoadFromAutoSlot()
    {
        string path = GetAutoSavePath();
        if (!TryReadSave(path, out GameSaveData data))
            return;

        BeginLoad(data);
    }

    public void DeleteRegularSlot(int slotIndex)
    {
        if (!IsRegularSlotIndexValid(slotIndex))
            return;

        string path = GetRegularSlotPath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            if (showLogs)
                Debug.Log($"[SaveSystem] Deleted slot {slotIndex}.", this);
        }

        OnSaveSlotsChanged?.Invoke();
    }

    public void DeleteAutoSlot()
    {
        string path = GetAutoSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            if (showLogs)
                Debug.Log("[SaveSystem] Deleted auto save.", this);
        }

        OnSaveSlotsChanged?.Invoke();
    }

    public void StartNewGame(string firstGameplaySceneName, string startEntryPointId)
    {
        pendingLoadData = null;
        pendingInitializeNewGameAfterSceneLoad = true;
        applyingPendingLoad = false;

        ResetRuntimeStateToDefaults();
        TrySwitchGameStateToPlaying();

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(firstGameplaySceneName, startEntryPointId);
            return;
        }

        SceneTransitionState.SetNextEntryPoint(startEntryPointId);
        SceneManager.LoadScene(firstGameplaySceneName, LoadSceneMode.Single);
    }

    private void BeginLoad(GameSaveData data)
    {
        if (data == null)
            return;

        if (string.IsNullOrWhiteSpace(data.sceneName))
        {
            Debug.LogWarning("[SaveSystem] Save file has empty scene name.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(data.sceneName))
        {
            Debug.LogError($"[SaveSystem] Scene '{data.sceneName}' is not in Build Settings.", this);
            return;
        }

        pendingLoadData = data;
        pendingInitializeNewGameAfterSceneLoad = false;
        applyingPendingLoad = false;

        TrySwitchGameStateToPlaying();

        if (showLogs)
            Debug.Log($"[SaveSystem] Loading scene from save: {data.sceneName}", this);

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(data.sceneName);
            return;
        }

        SceneManager.LoadScene(data.sceneName, LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadData != null)
        {
            StartCoroutine(ApplyPendingLoadRoutine());
            return;
        }

        if (pendingInitializeNewGameAfterSceneLoad)
        {
            StartCoroutine(ApplyPendingNewGameRoutine());
        }
    }

    private IEnumerator ApplyPendingNewGameRoutine()
    {
        pendingInitializeNewGameAfterSceneLoad = false;

        yield return null;
        yield return null;

        ResetRuntimeStateToDefaults();
    }

    private IEnumerator ApplyPendingLoadRoutine()
    {
        if (applyingPendingLoad)
            yield break;

        applyingPendingLoad = true;

        yield return null;
        yield return null;

        ApplyLoadedGameData(pendingLoadData);

        pendingLoadData = null;
        applyingPendingLoad = false;
    }

    private void ApplyLoadedGameData(GameSaveData data)
    {
        if (data == null)
            return;

        if (itemDatabase == null)
        {
            Debug.LogError("[SaveSystem] ItemDatabase is not assigned.", this);
            return;
        }

        InventorySystem inventory = ResolveInventorySystem();
        EquipmentSystem equipment = ResolveEquipmentSystem();
        ExpSystem expSystem = FindFirstObjectByType<ExpSystem>();
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        GameObject player = FindPlayerObject();

        if (inventory == null)
        {
            Debug.LogError("[SaveSystem] InventorySystem not found while applying load.", this);
            return;
        }

        if (equipment == null)
        {
            Debug.LogError("[SaveSystem] EquipmentSystem not found while applying load.", this);
            return;
        }

        if (expSystem == null)
        {
            Debug.LogError("[SaveSystem] ExpSystem not found while applying load.", this);
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("[SaveSystem] PlayerHealth not found while applying load.", this);
            return;
        }

        if (player == null)
        {
            Debug.LogError("[SaveSystem] Player object not found while applying load.", this);
            return;
        }

        equipment.ClearAllEquipment(false);
        inventory.ClearAllItems();

        expSystem.SetProgressFromSave(data.playerLevel, data.currentXP, data.xpToNextLevel);

        if (data.inventoryItems != null)
        {
            for (int i = 0; i < data.inventoryItems.Count; i++)
            {
                SaveItemEntryData entry = data.inventoryItems[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.itemId) || entry.amount <= 0)
                    continue;

                ItemData item = itemDatabase.GetItemById(entry.itemId);
                if (item == null)
                {
                    Debug.LogWarning($"[SaveSystem] Missing item in database: '{entry.itemId}'.", this);
                    continue;
                }

                inventory.AddItem(item, entry.amount);
            }
        }

        if (data.equippedItems != null)
        {
            for (int i = 0; i < data.equippedItems.Count; i++)
            {
                SaveEquipmentEntryData entry = data.equippedItems[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.itemId))
                    continue;

                ItemData item = itemDatabase.GetItemById(entry.itemId);
                if (item == null)
                {
                    Debug.LogWarning($"[SaveSystem] Missing equipped item in database: '{entry.itemId}'.", this);
                    continue;
                }

                equipment.SetItemInSlotDirect(item, entry.slotIndex, false);
            }
        }

        equipment.NotifyEquipmentChanged();
        playerHealth.SetCurrentHealthFromSave(data.currentHealth);

        Transform playerTransform = player.transform;
        playerTransform.position = data.playerPosition.ToVector3();

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (showLogs)
        {
            Debug.Log($"[SaveSystem] Save applied. Scene='{data.sceneName}', level={data.playerLevel}, hp={data.currentHealth}.", this);
        }
    }

    private GameSaveData CaptureCurrentGameData(bool isAutoSave, int slotIndex)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("[SaveSystem] ItemDatabase is not assigned.", this);
            return null;
        }

        InventorySystem inventory = ResolveInventorySystem();
        EquipmentSystem equipment = ResolveEquipmentSystem();
        ExpSystem expSystem = FindFirstObjectByType<ExpSystem>();
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        GameObject player = FindPlayerObject();

        if (inventory == null)
        {
            Debug.LogError("[SaveSystem] InventorySystem not found. Save aborted.", this);
            return null;
        }

        if (equipment == null)
        {
            Debug.LogError("[SaveSystem] EquipmentSystem not found. Save aborted.", this);
            return null;
        }

        if (expSystem == null)
        {
            Debug.LogError("[SaveSystem] ExpSystem not found. Save aborted.", this);
            return null;
        }

        if (playerHealth == null)
        {
            Debug.LogError("[SaveSystem] PlayerHealth not found. Save aborted.", this);
            return null;
        }

        if (player == null)
        {
            Debug.LogError("[SaveSystem] Player object not found. Save aborted.", this);
            return null;
        }

        GameSaveData data = new GameSaveData
        {
            isAutoSave = isAutoSave,
            slotIndex = slotIndex,
            savedAtUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            sceneName = SceneManager.GetActiveScene().name,
            playerPosition = new SerializableVector3(player.transform.position),
            playerLevel = expSystem.CurrentLvl,
            currentXP = expSystem.CurrentXP,
            xpToNextLevel = expSystem.XpToNextLvl,
            currentHealth = playerHealth.CurrentHealth,
            inventoryItems = new List<SaveItemEntryData>(),
            equippedItems = new List<SaveEquipmentEntryData>()
        };

        List<InventoryEntry> inventoryEntries = inventory.GetAllEntriesSnapshot();
        for (int i = 0; i < inventoryEntries.Count; i++)
        {
            InventoryEntry entry = inventoryEntries[i];
            if (entry == null || entry.Item == null || entry.Amount <= 0)
                continue;

            data.inventoryItems.Add(new SaveItemEntryData
            {
                itemId = entry.Item.ItemId,
                amount = entry.Amount
            });
        }

        for (int slot = 0; slot < equipment.SlotCount; slot++)
        {
            ItemData equippedItem = equipment.GetItemInSlot(slot);
            if (equippedItem == null)
                continue;

            data.equippedItems.Add(new SaveEquipmentEntryData
            {
                slotIndex = slot,
                itemId = equippedItem.ItemId
            });
        }

        return data;
    }

    private bool WriteSaveFile(string path, GameSaveData data)
    {
        try
        {
            EnsureSaveDirectoryExists();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);

            if (showLogs)
                Debug.Log($"[SaveSystem] Saved file: {path}", this);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveSystem] Failed to write save file.\nPath: {path}\n{exception}", this);
            return false;
        }
    }

    private bool TryReadSave(string path, out GameSaveData data)
    {
        data = null;

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"[SaveSystem] Save file is empty: {path}", this);
                return false;
            }

            data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null)
            {
                Debug.LogWarning($"[SaveSystem] Save file could not be parsed: {path}", this);
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveSystem] Failed to read save file.\nPath: {path}\n{exception}", this);
            return false;
        }
    }

    private void FillInfoFromSaveData(SaveSlotInfo info, GameSaveData data)
    {
        if (info == null || data == null)
            return;

        info.SceneName = string.IsNullOrWhiteSpace(data.sceneName) ? "Unknown Scene" : data.sceneName;
        info.PlayerLevel = Mathf.Max(1, data.playerLevel);
        info.SavedAtText = FormatUnixTime(data.savedAtUnixUtc);
    }

    private string FormatUnixTime(long unixTime)
    {
        if (unixTime <= 0)
            return "Unknown Time";

        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime();
        return dateTimeOffset.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    private void ResetRuntimeStateToDefaults()
    {
        EquipmentSystem equipment = ResolveEquipmentSystem();
        InventorySystem inventory = ResolveInventorySystem();
        ExpSystem expSystem = FindFirstObjectByType<ExpSystem>();
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

        equipment?.ClearAllEquipment(false);
        inventory?.ClearAllItems();
        expSystem?.ResetToDefaults();
        playerHealth?.RestoreToFull();
        equipment?.NotifyEquipmentChanged();
    }

    private void TrySwitchGameStateToPlaying()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.Playing);
        }
    }

    private InventorySystem ResolveInventorySystem()
    {
        if (InventorySystem.Instance != null)
            return InventorySystem.Instance;

        return FindFirstObjectByType<InventorySystem>();
    }

    private EquipmentSystem ResolveEquipmentSystem()
    {
        if (EquipmentSystem.Instance != null)
            return EquipmentSystem.Instance;

        return FindFirstObjectByType<EquipmentSystem>();
    }

    private GameObject FindPlayerObject()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            return player;

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        return playerHealth != null ? playerHealth.gameObject : null;
    }

    private bool IsRegularSlotIndexValid(int slotIndex)
    {
        return slotIndex >= 1 && slotIndex <= regularSlotCount;
    }

    private void EnsureSaveDirectoryExists()
    {
        string directory = GetSaveDirectory();
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private string GetSaveDirectory()
    {
        return Path.Combine(Application.persistentDataPath, "Saves");
    }

    private string GetRegularSlotPath(int slotIndex)
    {
        return Path.Combine(GetSaveDirectory(), $"slot_{slotIndex}.json");
    }

    private string GetAutoSavePath()
    {
        return Path.Combine(GetSaveDirectory(), "autosave.json");
    }
}