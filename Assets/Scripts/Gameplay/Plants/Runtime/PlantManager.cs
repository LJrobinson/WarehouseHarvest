using System.Collections.Generic;
using UnityEngine;
using Vertigro.Logic;

public class PlantManager : MonoBehaviour
{
    public List<GrowTable> allTables = new List<GrowTable>();

    private void Awake()
    {
        AutoFindTables();
    }

    public void AutoFindTables()
    {
        allTables.Clear();
        allTables.AddRange(FindObjectsByType<GrowTable>(FindObjectsInactive.Exclude));
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
                if (slot == null || slot.IsEmpty)
                    continue;

                if (slot.currentPlant == null)
                    continue;

                slot.currentPlant.AdvanceDay(lightMult, waterReduction);
            }
        }
    }
}