using System.Collections.Generic;
using UnityEngine;

public class SeedInventory : MonoBehaviour
{
    private List<SeedInstance> ownedSeeds = new List<SeedInstance>();

    public void AddSeed(SeedInstance seed)
    {
        if (seed == null || seed.strain == null)
            return;

        ownedSeeds.Add(seed);
        Debug.Log($"Added seed: {seed.DisplayName}");
    }

    public int GetSeedCount(PlantStrainData strain)
    {
        int count = 0;

        foreach (var seed in ownedSeeds)
        {
            if (seed.strain == strain)
                count++;
        }

        return count;
    }

    public SeedInstance ConsumeSeed(PlantStrainData strain)
    {
        for (int i = 0; i < ownedSeeds.Count; i++)
        {
            if (ownedSeeds[i].strain == strain)
            {
                SeedInstance seed = ownedSeeds[i];
                ownedSeeds.RemoveAt(i);

                Debug.Log($"Consumed seed: {seed.DisplayName}");
                return seed;
            }
        }

        Debug.Log($"No seeds available for {strain.strainName}");
        return null;
    }
}