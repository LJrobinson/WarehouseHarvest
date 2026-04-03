using System.Collections.Generic;
using UnityEngine;

public class GrowTable : MonoBehaviour
{
    [Header("Table Config")]
    public List<TableSlot> slots = new List<TableSlot>();
    public int unlockedSlots = 2;
    public LightQuality lightQuality = LightQuality.Basic;
    public WaterQuality waterQuality = WaterQuality.Tap;

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
}