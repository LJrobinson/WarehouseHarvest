using System.Collections.Generic;
using UnityEngine;

public class SeedInventory : MonoBehaviour
{
    private Dictionary<PlantStrainData, int> seeds = new Dictionary<PlantStrainData, int>();

    public int GetSeedCount(PlantStrainData strain)
    {
        if (strain == null) return 0;
        return seeds.ContainsKey(strain) ? seeds[strain] : 0;
    }

    public void AddSeeds(PlantStrainData strain, int amount)
    {
        if (strain == null || amount <= 0) return;

        if (!seeds.ContainsKey(strain))
            seeds[strain] = 0;

        seeds[strain] += amount;

        Debug.Log($"Added {amount} seeds of {strain.strainName}. Total: {seeds[strain]}");
    }

    public bool ConsumeSeed(PlantStrainData strain)
    {
        if (strain == null) return false;

        int count = GetSeedCount(strain);
        if (count <= 0)
        {
            Debug.Log($"No seeds available for {strain.strainName}");
            return false;
        }

        seeds[strain] -= 1;
        Debug.Log($"Consumed 1 seed of {strain.strainName}. Remaining: {seeds[strain]}");
        return true;
    }
}