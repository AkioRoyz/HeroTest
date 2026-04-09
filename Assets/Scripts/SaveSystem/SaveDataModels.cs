using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public bool isAutoSave;
    public int slotIndex;
    public long savedAtUnixUtc;

    public string sceneName;
    public SerializableVector3 playerPosition;

    public int playerLevel;
    public int currentXP;
    public int xpToNextLevel;
    public int currentHealth;

    public List<SaveItemEntryData> inventoryItems = new();
    public List<SaveEquipmentEntryData> equippedItems = new();
}

[Serializable]
public class SaveItemEntryData
{
    public string itemId;
    public int amount;
}

[Serializable]
public class SaveEquipmentEntryData
{
    public int slotIndex;
    public string itemId;
}

[Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3(Vector3 value)
    {
        x = value.x;
        y = value.y;
        z = value.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

public class SaveSlotInfo
{
    public bool Exists;
    public bool IsAutoSlot;
    public int SlotIndex;
    public string SceneName;
    public string SavedAtText;
    public int PlayerLevel;
    public bool IsCorrupted;
}