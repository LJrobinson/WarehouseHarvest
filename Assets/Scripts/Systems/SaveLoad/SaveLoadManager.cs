using System.IO;
using UnityEngine;
using Vertigro.Logic;

public class SaveLoadManager : MonoBehaviour
{
    [Header("References")]
    public DiscoveryManager discoveryManager;
    public EconomyManager economy;
    public PlayerStatsManager playerStats;
    public SeedInventory seedInventory;
    public StrainDatabase strainDatabase;
    public TimeManager timeManager;

    [Header("World References")]
    public Warehouse[] warehouses;

    [Header("Rack References")]
    public RackController[] racks;
    public TableController shelfUnitPrefab;
    public TowerManager towerManager;
    public HexSelectionController selectionController;

    [Header("Plant Prefab")]
    public PlantInstance plantPrefab;

    private float sessionStartTime;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    private void Start()
    {
        sessionStartTime = Time.time;
    }

    private float GetSessionPlayTime()
    {
        return Time.time - sessionStartTime;
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        // Save economy + time
        data.money = economy.Money;
        data.currentDay = timeManager.CurrentDay;

        if (MarketManager.Instance != null)
        {
            data.marketData = MarketManager.Instance.GetSaveData(timeManager.CurrentDay);
        }

        // Update and save player stats
        playerStats.AddPlaytimeFromSession();
        data.playerProfile = playerStats.GetSaveData();

        // Save discovered strains
        if (discoveryManager != null)
        {
            data.discoveredStrains = discoveryManager.GetDiscoveredStrainIDs();
        }

        // Save inventory seeds
        foreach (var seed in seedInventory.GetAllSeeds())
        {
            SeedSaveData seedData = new SeedSaveData();
            seedData.isMysterySeed = seed.isMysterySeed;
            seedData.isShiny = seed.isShiny;
            seedData.rarity = seed.rarity.ToString();
            seedData.strainName = seed.strain != null ? seed.strain.strainName : null;

            data.seeds.Add(seedData);
        }

        // Save warehouses + tables + plants
        if (warehouses != null)
        {
            foreach (Warehouse wh in warehouses)
            {
                if (wh == null)
                    continue;

                WarehouseSaveData whData = new WarehouseSaveData();
                whData.warehouseName = wh.name;
                whData.powerCapacityBonus = wh.BonusPowerCapacity;
                whData.waterCapacityBonus = wh.BonusWaterCapacity;
                whData.dataCapacityBonus = wh.BonusDataCapacity;

                foreach (GrowTable table in wh.tables)
                {
                    TableSaveData tableData = new TableSaveData();
                    tableData.tableName = table.name;

                    tableData.isUnlocked = table.isUnlocked;
                    tableData.unlockedSlots = table.unlockedSlots;
                    tableData.lightQuality = table.lightQuality.ToString();
                    tableData.waterQuality = table.waterQuality.ToString();

                    foreach (TableSlot slot in table.slots)
                    {
                        SlotSaveData slotData = new SlotSaveData();
                        slotData.hasPlant = !slot.IsEmpty;

                        if (!slot.IsEmpty)
                        {
                            PlantInstance plant = slot.currentPlant;

                            PlantSaveData plantData = new PlantSaveData();
                            plantData.waterLevel = plant.waterLevel;
                            plantData.nutrientsLevel = plant.nutrientsLevel;
                            plantData.stress = plant.stress;
                            plantData.health = plant.health;

                            plantData.hasMold = plant.hasMold;
                            plantData.hasPests = plant.hasPests;

                            plantData.growthPercent = plant.growthPercent;
                            plantData.ripenessPercent = plant.ripenessPercent;

                            plantData.stage = plant.stage.ToString();

                            // Save plant seed
                            SeedSaveData plantSeed = new SeedSaveData();
                            plantSeed.isMysterySeed = plant.seed.isMysterySeed;
                            plantSeed.isShiny = plant.seed.isShiny;
                            plantSeed.rarity = plant.seed.rarity.ToString();
                            plantSeed.strainName = plant.seed.strain != null ? plant.seed.strain.strainName : null;

                            plantData.seed = plantSeed;

                            slotData.plant = plantData;
                        }

                        tableData.slots.Add(slotData);
                    }

                    whData.tables.Add(tableData);
                }

                data.warehouses.Add(whData);
            }
        }

        SaveRackStates(data);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        //Reset Timer
        sessionStartTime = Time.time;

        Debug.Log($"GAME SAVED: {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        if (data == null)
        {
            Debug.LogError("Save file could not be read.");
            return;
        }

        // Restore economy + time
        economy.SetMoney(data.money);
        timeManager.SetDay(data.currentDay);

        // Restore market
        if (MarketManager.Instance != null && data.marketData != null)
        {
            MarketManager.Instance.LoadFromSaveData(data.marketData);
        }

        // Restore player profile
        if (data.playerProfile != null)
            playerStats.LoadFromSave(data.playerProfile);
        else
            playerStats.LoadFromSave(new PlayerProfileSaveData());

        // Restore discovered strains
        if (discoveryManager != null)
        {
            discoveryManager.LoadDiscoveredStrainIDs(data.discoveredStrains);
        }

        // Restore inventory
        seedInventory.ClearAllSeeds();

        foreach (var seedData in data.seeds)
        {
            SeedInstance seed = ConvertSeed(seedData);
            seedInventory.AddSeed(seed);
        }

        // Restore warehouses + tables + plants
        if (data.warehouses != null)
        {
            foreach (var whData in data.warehouses)
            {
                Warehouse warehouse = FindWarehouseByName(whData.warehouseName);
                if (warehouse == null)
                    continue;

                warehouse.SetUtilityCapacityBonuses(
                    whData.powerCapacityBonus,
                    whData.waterCapacityBonus,
                    whData.dataCapacityBonus);

                foreach (var tableData in whData.tables)
                {
                    GrowTable table = FindTableByName(warehouse, tableData.tableName);
                    if (table == null)
                        continue;

                    table.isUnlocked = tableData.isUnlocked;
                    table.unlockedSlots = tableData.unlockedSlots;

                    if (System.Enum.TryParse(tableData.lightQuality, out LightQuality lq))
                        table.lightQuality = lq;

                    if (System.Enum.TryParse(tableData.waterQuality, out WaterQuality wq))
                        table.waterQuality = wq;

                    // Clear existing plants
                    foreach (var slot in table.slots)
                        slot.RemovePlant();

                    // Restore plants
                    for (int i = 0; i < tableData.slots.Count && i < table.slots.Count; i++)
                    {
                        SlotSaveData slotData = tableData.slots[i];

                        if (!slotData.hasPlant || slotData.plant == null)
                            continue;

                        TableSlot slot = table.slots[i];

                        PlantInstance plant = Instantiate(plantPrefab, slot.transform.position, Quaternion.identity);
                        plant.transform.SetParent(slot.transform);

                        // Restore seed
                        SeedInstance seed = ConvertSeed(slotData.plant.seed);
                        plant.seed = seed;

                        // Restore plant stats
                        plant.waterLevel = slotData.plant.waterLevel;
                        plant.nutrientsLevel = slotData.plant.nutrientsLevel;
                        plant.stress = slotData.plant.stress;
                        plant.health = slotData.plant.health;

                        plant.hasMold = slotData.plant.hasMold;
                        plant.hasPests = slotData.plant.hasPests;

                        plant.growthPercent = slotData.plant.growthPercent;
                        plant.ripenessPercent = slotData.plant.ripenessPercent;

                        if (System.Enum.TryParse(slotData.plant.stage, out PlantStage stage))
                            plant.stage = stage;

                        plant.ApplyVisuals();

                        slot.currentPlant = plant;
                    }
                }
            }
        }

        RestoreRackStates(data);

        Debug.Log("GAME LOADED SUCCESSFULLY.");
    }

    private void SaveRackStates(GameSaveData data)
    {
        if (data == null || racks == null)
            return;

        foreach (RackController rack in racks)
        {
            if (rack == null)
                continue;

            RackSaveData rackData = new RackSaveData();
            rackData.rackName = rack.name;

            for (int slotIndex = 1; slotIndex <= RackController.ShelfSlotCount; slotIndex++)
            {
                ShelfSlotRecord slot = rack.GetShelfSlot(slotIndex);
                if (slot == null)
                    continue;

                RackShelfSlotSaveData slotData = new RackShelfSlotSaveData();
                slotData.slotIndex = slotIndex;
                slotData.isUnlocked = slot.isUnlocked;
                slotData.isActive = slot.isUnlocked && slot.shelf != null;
                slotData.shelfLevel = slot.shelf != null ? Mathf.Max(1, slot.shelf.currentShelfLevel) : 1;

                rackData.shelfSlots.Add(slotData);
            }

            data.racks.Add(rackData);
        }
    }

    private void RestoreRackStates(GameSaveData data)
    {
        if (racks == null)
            return;

        foreach (RackController rack in racks)
        {
            if (rack == null)
                continue;

            RackSaveData rackData = FindRackSaveData(data, rack.name);
            RestoreRackState(rack, rackData);
        }
    }

    private void RestoreRackState(RackController rack, RackSaveData rackData)
    {
        if (rack == null)
            return;

        for (int slotIndex = 1; slotIndex <= RackController.ShelfSlotCount; slotIndex++)
        {
            ShelfSlotRecord slot = rack.GetShelfSlot(slotIndex);
            if (slot == null)
                continue;

            RackShelfSlotSaveData slotData = FindRackShelfSlotSaveData(rackData, slotIndex);
            bool shouldBeUnlocked = slotIndex == 1 || (slotData != null && slotData.isUnlocked);
            bool shouldBeActive = slotIndex == 1 || (slotData != null && slotData.isActive);
            int shelfLevel = slotData != null ? Mathf.Max(1, slotData.shelfLevel) : 1;

            if (slotIndex == 1)
            {
                RestoreStarterShelfSlot(rack, slot, shelfLevel);
                continue;
            }

            rack.ClearShelfSlotInstance(slotIndex, towerManager);
            slot.isUnlocked = shouldBeUnlocked;

            if (shouldBeUnlocked && shouldBeActive)
            {
                rack.TryActivateShelfSlot(
                    slotIndex,
                    shelfUnitPrefab,
                    towerManager,
                    selectionController,
                    shelfLevel,
                    out _);
            }
        }
    }

    private static void RestoreStarterShelfSlot(RackController rack, ShelfSlotRecord slot, int shelfLevel)
    {
        if (slot == null)
            return;

        slot.isUnlocked = true;

        if (slot.shelf == null)
            return;

        int clampedShelfLevel = Mathf.Clamp(shelfLevel, 1, Mathf.Max(1, slot.shelf.maxShelfLevel));

        if (slot.shelf.currentShelfLevel != clampedShelfLevel)
        {
            slot.shelf.currentShelfLevel = clampedShelfLevel;
            slot.shelf.BuildTable();
        }

        slot.shelf.SetUtilityStateSource(rack);
    }

    private static RackSaveData FindRackSaveData(GameSaveData data, string rackName)
    {
        if (data == null || data.racks == null || string.IsNullOrEmpty(rackName))
            return null;

        foreach (RackSaveData rackData in data.racks)
        {
            if (rackData != null && rackData.rackName == rackName)
                return rackData;
        }

        return null;
    }

    private static RackShelfSlotSaveData FindRackShelfSlotSaveData(RackSaveData rackData, int slotIndex)
    {
        if (rackData == null || rackData.shelfSlots == null)
            return null;

        foreach (RackShelfSlotSaveData slotData in rackData.shelfSlots)
        {
            if (slotData != null && slotData.slotIndex == slotIndex)
                return slotData;
        }

        return null;
    }

    private SeedInstance ConvertSeed(SeedSaveData seedData)
    {
        SeedInstance seed = new SeedInstance();
        seed.isMysterySeed = seedData.isMysterySeed;
        seed.isShiny = seedData.isShiny;

        if (System.Enum.TryParse(seedData.rarity, out SeedRarity rarity))
            seed.rarity = rarity;

        if (!string.IsNullOrEmpty(seedData.strainName))
            seed.strain = strainDatabase.GetStrainByName(seedData.strainName);
        else
            seed.strain = null;

        return seed;
    }

    private Warehouse FindWarehouseByName(string name)
    {
        if (warehouses == null)
            return null;

        foreach (var wh in warehouses)
            if (wh != null && wh.name == name)
                return wh;

        return null;
    }

    private GrowTable FindTableByName(Warehouse wh, string tableName)
    {
        foreach (var t in wh.tables)
            if (t.name == tableName)
                return t;

        return null;
    }
}
