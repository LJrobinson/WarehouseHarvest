using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public List<GrowTable> allTables = new List<GrowTable>();

    // Advance day for all plants across all tables
    public void AdvanceDayAll()
    {
        foreach (var table in allTables)
        {
            float lightMult = table.GetLightMultiplier();
            float waterReduction = table.GetWaterStressReduction();

            foreach (var slot in table.slots)
            {
                if (!slot.IsEmpty)
                {
                    slot.currentPlant.AdvanceDay(lightMult, waterReduction);
                }
            }
        }
    }

    // Plant seed into first available slot on any table
    public bool PlantSeedAnywhere(PlantInstance seedPrefab)
    {
        foreach (var table in allTables)
        {
            List<TableSlot> available = table.GetAvailableSlots();
            if (available.Count > 0)
            {
                available[0].PlantSeed(seedPrefab);
                return true;
            }
        }
        Debug.LogWarning("No available slots on any table!");
        return false;
    }
}