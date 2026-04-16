using UnityEngine;

public class SeedShop : MonoBehaviour
{
    public EconomyManager economy;
    public SeedInventory seedInventory;
    public StrainDatabase strainDatabase;

    [Header("Bagseed Settings")]
    public int bagseedSingleCost = 15;
    public int bagseedPack5Cost = 60;
    public int bagseedPack20Cost = 200;

    public bool BuySingleSeed(PlantStrainData strain)
    {
        if (strain == null)
            return false;

        int cost = strain.seedCost;

        if (!economy.SpendMoney(cost))
            return false;

        SeedInstance seed = SeedGenerator.GenerateSeed(strain);
        seedInventory.AddSeed(seed);
        StrainStatsManager.Instance.RecordSeedPurchase(strain.strainName, cost);
        Debug.Log($"Bought 1 seed: {seed.DisplayName} for ${cost}");
        return true;
    }

    public bool BuyPack(PlantStrainData strain, int amount)
    {
        if (strain == null)
            return false;

        int cost = amount switch
        {
            5 => strain.pack5Cost,
            20 => strain.pack20Cost,
            _ => strain.seedCost * amount
        };

        if (!economy.SpendMoney(cost))
            return false;

        for (int i = 0; i < amount; i++)
        {
            SeedInstance seed = SeedGenerator.GenerateSeed(strain);
            seedInventory.AddSeed(seed);
        }
        StrainStatsManager.Instance.RecordSeedPurchase(strain.strainName, cost);
        Debug.Log($"Bought {amount}-pack of {strain.strainName} for ${cost}");
        return true;
    }

    public bool BuyBagseedSingle()
    {
        if (!economy.SpendMoney(bagseedSingleCost))
            return false;

        SeedInstance seed = SeedGenerator.GenerateMysterySeed(strainDatabase);
        seedInventory.AddSeed(seed);
        StrainStatsManager.Instance.RecordSeedPurchase("MYSTERY", bagseedSingleCost);
        Debug.Log($"Bought 1 bagseed for ${bagseedSingleCost}");
        return true;
    }

    public bool BuyBagseedPack(int amount)
    {
        int cost = amount switch
        {
            5 => bagseedPack5Cost,
            20 => bagseedPack20Cost,
            _ => bagseedSingleCost * amount
        };

        if (!economy.SpendMoney(cost))
            return false;

        for (int i = 0; i < amount; i++)
        {
            SeedInstance seed = SeedGenerator.GenerateMysterySeed(strainDatabase);
            seedInventory.AddSeed(seed);
        }
        StrainStatsManager.Instance.RecordSeedPurchase("MYSTERY", bagseedSingleCost);
        Debug.Log($"Bought bagseed pack ({amount}) for ${cost}");
        return true;
    }
}