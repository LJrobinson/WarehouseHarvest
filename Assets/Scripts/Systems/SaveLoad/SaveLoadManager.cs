using System.IO;
using UnityEngine;

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
        foreach (Warehouse wh in warehouses)
        {
            WarehouseSaveData whData = new WarehouseSaveData();
            whData.warehouseName = wh.name;

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
        foreach (var whData in data.warehouses)
        {
            Warehouse warehouse = FindWarehouseByName(whData.warehouseName);
            if (warehouse == null)
                continue;

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

        Debug.Log("GAME LOADED SUCCESSFULLY.");
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
        foreach (var wh in warehouses)
            if (wh.name == name)
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