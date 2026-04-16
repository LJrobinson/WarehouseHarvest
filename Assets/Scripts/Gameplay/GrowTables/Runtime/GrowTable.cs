using System.Collections.Generic;
using UnityEngine;
using Vertigro.Logic;

public class GrowTable : MonoBehaviour
{
    private const int HexColumns = 6;

    [Header("Unlocking")]
    public bool isUnlocked = false;

    [Header("Table Config")]
    public int unlockedSlots = 2;

    [Header("Hex Grid Visual")]
    public TableGenerator tableGenerator;
    public int floorIndex = 0;

    [Header("Legacy Slot Bridge (UI compatibility)")]
    public List<TableSlot> slots = new List<TableSlot>();

    [Header("Warehouse Utility Status")]
    public bool utilitiesOnline = true;

    [Header("Warehouse Utility Usage")]
    public float dataUsage = 10f;
    public float waterUsage = 25f;
    public float powerUsage = 50f;

    [Header("Utility Priority")]
    [Range(0, 100)]
    public int manualPriority = 50;

    [Header("Quality Systems")]
    public LightQuality lightQuality = LightQuality.Basic;
    public WaterQuality waterQuality = WaterQuality.Tap;

    private void Start()
    {
        if (isUnlocked)
            RefreshHexGrid();
    }

    public void RefreshHexGrid()
    {
        if (tableGenerator == null)
            return;

        int rows = Mathf.CeilToInt(unlockedSlots / (float)HexColumns);
        if (rows <= 0)
            return;

        tableGenerator.GenerateTable(rows, HexColumns, floorIndex);
    }

    public void UpgradeSlots(int extraSlots)
    {
        unlockedSlots += extraSlots;
        RefreshHexGrid();
    }

    public bool UnlockTable(EconomyManager economy)
    {
        isUnlocked = true;
        RefreshHexGrid();
        return true;
    }

    public bool UpgradeSlots(EconomyManager economy, int extraSlots)
    {
        unlockedSlots += extraSlots;
        RefreshHexGrid();
        return true;
    }

    public bool UpgradeLights(EconomyManager economy)
    {
        return true;
    }

    public bool UpgradeWater(EconomyManager economy)
    {
        return true;
    }

    public int GetSlotCount()
    {
        return slots != null ? slots.Count : 0;
    }

    // -----------------------------
    // REQUIRED BY PlantManager
    // -----------------------------

    public float GetLightMultiplier()
    {
        return lightQuality switch
        {
            LightQuality.Basic => 1f,
            LightQuality.LED => 1.1f,
            LightQuality.HighEndLED => 1.25f,
            LightQuality.Commercial => 1.45f,
            _ => 1f
        };
    }

    public float GetWaterStressReduction()
    {
        return waterQuality switch
        {
            WaterQuality.Tap => 0f,
            WaterQuality.Filtered => 0.5f,
            WaterQuality.ReverseOsmosis => 1.5f,
            WaterQuality.UltraPure => 3f,
            _ => 0f
        };
    }
}