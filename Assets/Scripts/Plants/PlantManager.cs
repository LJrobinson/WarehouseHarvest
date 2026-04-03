using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public List<GrowTable> allTables = new List<GrowTable>();

    private void Awake()
    {
        AutoRegisterTables();
    }

    [ContextMenu("Auto Register Tables")]
    public void AutoRegisterTables()
    {
        allTables.Clear();
        allTables.AddRange(FindObjectsByType<GrowTable>());

        Debug.Log($"PlantManager registered {allTables.Count} GrowTables.");
    }

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
}