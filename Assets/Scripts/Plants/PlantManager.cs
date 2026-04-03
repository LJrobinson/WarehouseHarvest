using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public List<GrowTable> allTables = new List<GrowTable>();

    private void Awake()
    {
        AutoFindTables();
    }

    [ContextMenu("Auto Find Tables")]
    public void AutoFindTables()
    {
        allTables.Clear();
        allTables.AddRange(FindObjectsByType<GrowTable>(FindObjectsInactive.Exclude));

        Debug.Log($"PlantManager found {allTables.Count} tables.");
    }

    public void AdvanceDayAll()
    {
        foreach (var table in allTables)
        {
            if (table == null || !table.isUnlocked)
                continue;

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
}