using System.Collections.Generic;
using UnityEngine;


public class GrowTable : MonoBehaviour
{
    [Header("Unlocking")]
    public bool isUnlocked = false;

    [Header("Upgrade Costs")]
    public int unlockTableCost = 500;
    public int slotUpgradeCost = 150;
    public int lightUpgradeCost = 250;
    public int waterUpgradeCost = 200;

    [Header("Table Config")]
    public List<TableSlot> slots = new List<TableSlot>();
    public int unlockedSlots = 2;
    public LightQuality lightQuality = LightQuality.Basic;
    public WaterQuality waterQuality = WaterQuality.Tap;

    [Header("Warehouse Utility Status")]
    public bool utilitiesOnline = true;

    [Header("Warehouse Utility Usage")]
    public float dataUsage = 10f;
    public float powerUsage = 50f;
    public float waterUsage = 25f;
    
    [Header("Utility Priority")]
    [Range(0, 100)]
    public int manualPriority = 50;

    public List<TableSlot> GetAvailableSlots()
    {
        List<TableSlot> available = new List<TableSlot>();
        for (int i = 0; i < unlockedSlots && i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
                available.Add(slots[i]);
        }
        return available;
    }

    public void UpgradeSlots(int extraSlots)
    {
        unlockedSlots = Mathf.Clamp(unlockedSlots + extraSlots, 1, slots.Count);
        Debug.Log($"Table upgraded! Slots unlocked: {unlockedSlots}/{slots.Count}");
    }

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

    public bool UnlockTable(EconomyManager economy)
    {
        if (isUnlocked)
            return false;

        if (!economy.SpendMoney(unlockTableCost))
            return false;

        isUnlocked = true;
        Debug.Log($"{name} unlocked!");
        return true;
    }

    public bool UpgradeSlots(EconomyManager economy, int extraSlots)
    {
        if (!isUnlocked)
            return false;

        if (!economy.SpendMoney(slotUpgradeCost))
            return false;

        unlockedSlots = Mathf.Clamp(unlockedSlots + extraSlots, 1, slots.Count);
        Debug.Log($"{name} slots upgraded: {unlockedSlots}/{slots.Count}");
        return true;
    }

    public bool UpgradeLights(EconomyManager economy)
    {
        if (!isUnlocked)
            return false;

        if (!economy.SpendMoney(lightUpgradeCost))
            return false;

        if (lightQuality == LightQuality.Commercial)
            return false;

        lightQuality++;
        Debug.Log($"{name} lights upgraded to {lightQuality}");
        return true;
    }

    public bool UpgradeWater(EconomyManager economy)
    {
        if (!isUnlocked)
            return false;

        if (!economy.SpendMoney(waterUpgradeCost))
            return false;

        if (waterQuality == WaterQuality.UltraPure)
            return false;

        waterQuality++;
        Debug.Log($"{name} water upgraded to {waterQuality}");
        return true;
    }
}