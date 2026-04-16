using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int currentDay;
    public int money;

    public PlayerProfileSaveData playerProfile = new PlayerProfileSaveData();

    public List<string> discoveredStrains = new List<string>();

    public List<SeedSaveData> seeds = new List<SeedSaveData>();
    public List<WarehouseSaveData> warehouses = new List<WarehouseSaveData>();

    public MarketSaveData marketData;
}

[Serializable]
public class PlayerProfileSaveData
{
    public string playerName;
    public string steamName;

    public float totalPlayTimeSeconds;

    public int totalPlantsGrown;
    public int totalHarvests;

    public int totalMoneyEarned;
    public int totalMoneySpent;

    public int totalStrainsUnlocked;
}

[Serializable]
public class SeedSaveData
{
    public string strainName; // null if mystery seed
    public bool isMysterySeed;
    public string rarity;
    public bool isShiny;
}

[Serializable]
public class WarehouseSaveData
{
    public string warehouseName;
    public List<TableSaveData> tables = new List<TableSaveData>();
}

[Serializable]
public class TableSaveData
{
    public string tableName;

    public bool isUnlocked;
    public int unlockedSlots;

    public string lightQuality;
    public string waterQuality;

    public List<SlotSaveData> slots = new List<SlotSaveData>();
}

[Serializable]
public class SlotSaveData
{
    public bool hasPlant;
    public PlantSaveData plant;
}

[Serializable]
public class PlantSaveData
{
    public float waterLevel;
    public float nutrientsLevel;
    public float stress;
    public float health;

    public bool hasMold;
    public bool hasPests;

    public float growthPercent;
    public float ripenessPercent;

    public string stage;

    public SeedSaveData seed;
}