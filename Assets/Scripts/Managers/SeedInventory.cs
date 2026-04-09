using System.Collections.Generic;
using UnityEngine;

public class SeedInventory : MonoBehaviour
{
    private List<SeedInstance> ownedSeeds = new List<SeedInstance>();

    public void AddSeed(SeedInstance seed)
    {
        if (seed == null)
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

    public List<SeedInstance> GetAllSeeds()
    {
        return ownedSeeds;
    }

    public bool RemoveSpecificSeed(SeedInstance seed)
    {
        return ownedSeeds.Remove(seed);
    }

    public List<SeedInventorySummary> GetSummaries()
    {
        Dictionary<string, SeedInventorySummary> map = new Dictionary<string, SeedInventorySummary>();

        foreach (SeedInstance seed in ownedSeeds)
        {
            bool mystery = seed.isMysterySeed;
            string key = mystery ? "MYSTERY" : seed.strain != null ? seed.strain.strainName : "UNKNOWN";

            if (!map.ContainsKey(key))
            {
                map[key] = new SeedInventorySummary()
                {
                    strain = seed.strain,
                    isMystery = mystery,
                    totalCount = 0,
                    shinyCount = 0,
                    rarityCounts = new Dictionary<SeedRarity, int>()
                };

                foreach (SeedRarity r in System.Enum.GetValues(typeof(SeedRarity)))
                    map[key].rarityCounts[r] = 0;
            }

            map[key].totalCount++;

            map[key].rarityCounts[seed.rarity]++;

            if (seed.isShiny)
                map[key].shinyCount++;
        }

        return new List<SeedInventorySummary>(map.Values);
    }

    public List<SeedStack> GetSeedStacks()
    {
        Dictionary<string, SeedStack> map = new Dictionary<string, SeedStack>();

        foreach (SeedInstance seed in ownedSeeds)
        {
            string key = $"{seed.strain?.strainName}_{seed.rarity}_{seed.isMysterySeed}";

            if (!map.ContainsKey(key))
            {
                map[key] = new SeedStack()
                {
                    strain = seed.strain,
                    rarity = seed.rarity,
                    isMystery = seed.isMysterySeed,
                    count = 0
                };
            }

            map[key].count++;
        }

        return new List<SeedStack>(map.Values);
    }

    public void ClearAllSeeds()
    {
        ownedSeeds.Clear();
    }
}