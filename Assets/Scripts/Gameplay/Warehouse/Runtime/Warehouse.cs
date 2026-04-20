using System.Collections.Generic;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    

    [Header("Warehouse Visuals")]
    public Sprite warehouseIcon;

    [Header("Warehouse Info")]
    public string warehouseName = "Warehouse 01";

    [Header("Real Estate Info")]
    public int purchaseCost = 10000;

    [TextArea]
    public string description = "A basic warehouse space.";

    [Header("Utilities (Upgrade Levels)")]
    [Range(1, 6)] public int dataLevel = 1;
    [Range(1, 6)] public int powerLevel = 1;
    [Range(1, 6)] public int waterLevel = 1;
    
    [Header("Utilities (Capacity)")]
    public float currentData;
    public float currentPower;
    public float currentWater;
    public float maxData = 100;
    public float maxPower = 100;
    public float maxWater = 100;

    [Header("Tables")]
    public List<GrowTable> tables = new List<GrowTable>();

    [Header("Utility Allocation Mode")]
    public WarehouseAllocationMode allocationMode = WarehouseAllocationMode.Balanced;

    [Header("Upgrade Settings")]
    public int maxUpgradeLevel = 6;

    public float[] dataUpgradeValues = { 250, 500, 1000, 2000, 4000, 8000 };
    public float[] powerUpgradeValues = { 250, 500, 1000, 2000, 4000, 8000 };
    public float[] waterUpgradeValues = { 250, 500, 1000, 2000, 4000, 8000 };

    private void Awake()
    {
        if (tables == null)
            tables = new List<GrowTable>();

        if (tables.Count == 0)
        {
            tables.AddRange(GetComponentsInChildren<GrowTable>());
        }

        tables.RemoveAll(t => t == null);

        if (tables.Count > 0)
        {
            tables[0].isUnlocked = true;
        }

        ApplyUpgradeCaps();

        currentData = maxData;
        currentPower = maxPower;
        currentWater = maxWater;
    }

    public void ApplyUpgradeCaps()
    {
        maxData = dataUpgradeValues[Mathf.Clamp(dataLevel - 1, 0, dataUpgradeValues.Length - 1)];
        maxPower = powerUpgradeValues[Mathf.Clamp(powerLevel - 1, 0, powerUpgradeValues.Length - 1)];
        maxWater = waterUpgradeValues[Mathf.Clamp(waterLevel - 1, 0, waterUpgradeValues.Length - 1)];

        currentData = Mathf.Clamp(currentData, 0, maxData);
        currentPower = Mathf.Clamp(currentPower, 0, maxPower);
        currentWater = Mathf.Clamp(currentWater, 0, maxWater);
        
    }

    public bool UpgradeData()
    {
        if (dataLevel >= maxUpgradeLevel) return false;
        dataLevel++;
        ApplyUpgradeCaps();
        return true;
    }

    public bool UpgradePower()
    {
        if (powerLevel >= maxUpgradeLevel) return false;
        powerLevel++;
        ApplyUpgradeCaps();
        return true;
    }

    public bool UpgradeWater()
    {
        if (waterLevel >= maxUpgradeLevel) return false;
        waterLevel++;
        ApplyUpgradeCaps();
        return true;
    }

    public void AddPowerCapacity(float amount)
    {
        maxPower = GetIncreasedCapacity(maxPower, amount);
        currentPower = ClampCurrentUtility(currentPower, maxPower);
    }

    public void AddWaterCapacity(float amount)
    {
        maxWater = GetIncreasedCapacity(maxWater, amount);
        currentWater = ClampCurrentUtility(currentWater, maxWater);
    }

    public void AddDataCapacity(float amount)
    {
        maxData = GetIncreasedCapacity(maxData, amount);
        currentData = ClampCurrentUtility(currentData, maxData);
    }

    public float GetTotalDataUsage()
    {
        float total = 0f;

        foreach (var table in tables)
        {
            if (table == null) continue;
            if (!table.isUnlocked) continue;

            total += table.dataUsage;
        }

        return total;
    }

    public float GetTotalPowerUsage()
    {
        float total = 0f;

        foreach (var table in tables)
        {
            if (table == null) continue;
            if (!table.isUnlocked) continue;

            total += table.powerUsage;
        }

        return total;
    }

    public float GetTotalWaterUsage()
    {
        float total = 0f;

        foreach (var table in tables)
        {
            if (table == null) continue;
            if (!table.isUnlocked) continue;

            total += table.waterUsage;
        }

        return total;
    }

    public bool HasEnoughUtilities()
    {
        return GetTotalDataUsage() <= maxData &&
               GetTotalPowerUsage() <= maxPower &&
               GetTotalWaterUsage() <= maxWater;
               
    }

    public void UpdateUtilityStatusForTables()
    {
        float dataCap = maxData;
        float powerCap = maxPower;
        float waterCap = maxWater;

        float dataUsed = 0f;
        float powerUsed = 0f;
        float waterUsed = 0f;

        List<GrowTable> unlockedTables = new List<GrowTable>();

        foreach (GrowTable table in tables)
        {
            if (table == null)
                continue;

            if (!table.isUnlocked)
            {
                table.utilitiesOnline = false;
                continue;
            }

            unlockedTables.Add(table);
        }

        unlockedTables.Sort((a, b) =>
            GetTablePriorityScore(b).CompareTo(GetTablePriorityScore(a))
        );

        foreach (GrowTable table in unlockedTables)
        {
            bool canSupport =
                (dataUsed + table.dataUsage <= dataCap) &&
                (powerUsed + table.powerUsage <= powerCap) &&
                (waterUsed + table.waterUsage <= waterCap);

            if (canSupport)
            {
                table.utilitiesOnline = true;
                dataUsed += table.dataUsage;
                powerUsed += table.powerUsage;
                waterUsed += table.waterUsage;
            }
            else
            {
                table.utilitiesOnline = false;
            }
        }
    }

    private void Update()
    {
        UpdateUtilityStatusForTables();
    }

    private float GetTablePriorityScore(GrowTable table)
    {
        if (table == null)
            return -9999;

        if (!table.isUnlocked)
            return -9999;

        // Manual mode = literal player priority
        if (allocationMode == WarehouseAllocationMode.ManualPriority)
            return table.manualPriority;

        int plantsGrowing = 0;
        int harvestable = 0;

        for (int i = 0; i < table.unlockedSlots && i < table.slots.Count; i++)
        {
            TableSlot slot = table.slots[i];
            if (slot == null || slot.currentPlant == null)
                continue;

            plantsGrowing++;

            if (slot.currentPlant.IsHarvestable)
                harvestable++;
        }

        switch (allocationMode)
        {
            case WarehouseAllocationMode.Balanced:
                // Keeps more tables online (lower usage tables naturally win)
                return 1000f - (table.powerUsage + table.waterUsage + table.dataUsage);

            case WarehouseAllocationMode.PlantsFirst:
                return plantsGrowing * 100f;

            case WarehouseAllocationMode.HarvestFirst:
                return harvestable * 500f + plantsGrowing * 10f;

            case WarehouseAllocationMode.HighValueFirst:
                return CalculateHighValueScore(table);

            default:
                return 0;
        }
    }

    private float CalculateHighValueScore(GrowTable table)
    {
        float score = 0f;

        for (int i = 0; i < table.unlockedSlots && i < table.slots.Count; i++)
        {
            TableSlot slot = table.slots[i];
            if (slot == null || slot.currentPlant == null)
                continue;

            PlantInstance plant = slot.currentPlant;

            if (plant.seed == null)
                continue;

            // Base rarity scoring
            score += plant.seed.rarity switch
            {
                SeedRarity.Common => 10,
                SeedRarity.Uncommon => 25,
                SeedRarity.Rare => 50,
                SeedRarity.Epic => 100,
                SeedRarity.Legendary => 200,
                _ => 10
            };

            // Shiny bonus
            if (plant.seed.isShiny)
                score += 500;

            // Harvestable bonus (protect harvest-ready plants)
            if (plant.IsHarvestable)
                score += 250;
        }

        return score;
    }

    private static float GetIncreasedCapacity(float currentCapacity, float amount)
    {
        return GetSafeUtilityValue(currentCapacity) + GetSafeUtilityValue(amount);
    }

    private static float ClampCurrentUtility(float currentValue, float maxValue)
    {
        return Mathf.Clamp(GetSafeUtilityValue(currentValue), 0f, Mathf.Max(0f, maxValue));
    }

    private static float GetSafeUtilityValue(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return 0f;

        return Mathf.Max(0f, value);
    }
}
