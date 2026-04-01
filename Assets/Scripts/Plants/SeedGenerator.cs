using UnityEngine;

public static class SeedGenerator
{
    public static SeedInstance GenerateSeed(PlantStrainData strain, float rarityBoost)
    {
        SeedInstance seed = new SeedInstance();
        seed.strain = strain;

        seed.rarity = RollRarity(rarityBoost);
        seed.isShiny = Random.value < strain.shinyChance;

        seed.geneticsBonus = GetGeneticsBonus(seed.rarity, seed.isShiny);

        return seed;
    }

    public static SeedInstance GenerateSeed(PlantStrainData strain)
    {
        return GenerateSeed(strain, 0f);
    }

    private static SeedRarity RollRarity(float rarityBoost)
    {
        // rarityBoost shifts odds slightly toward higher tiers
        float roll = Random.value - rarityBoost;
        roll = Mathf.Clamp01(roll);

        if (roll < 0.50f) return SeedRarity.Common;
        if (roll < 0.75f) return SeedRarity.Uncommon;
        if (roll < 0.90f) return SeedRarity.Rare;
        if (roll < 0.98f) return SeedRarity.Epic;
        return SeedRarity.Legendary;
    }

    private static int GetGeneticsBonus(SeedRarity rarity, bool shiny)
    {
        int bonus = rarity switch
        {
            SeedRarity.Common => 0,
            SeedRarity.Uncommon => 15,
            SeedRarity.Rare => 35,
            SeedRarity.Epic => 60,
            SeedRarity.Legendary => 90,
            _ => 0
        };

        if (shiny)
            bonus += 50;

        return bonus;
    }

    public static float GetPayoutMultiplierBonus(SeedRarity rarity, bool shiny)
    {
        float mult = rarity switch
        {
            SeedRarity.Common => 1.0f,
            SeedRarity.Uncommon => 1.05f,
            SeedRarity.Rare => 1.15f,
            SeedRarity.Epic => 1.30f,
            SeedRarity.Legendary => 1.50f,
            _ => 1.0f
        };

        if (shiny)
            mult *= 1.25f;

        return mult;
    }

    public static SeedInstance GenerateSeedWithMinimumRarity(PlantStrainData strain, SeedRarity minimumRarity, float rarityBoost)
    {
        SeedInstance seed = GenerateSeed(strain, rarityBoost);

        if (seed.rarity < minimumRarity)
            seed.rarity = minimumRarity;

        seed.geneticsBonus = GetGeneticsBonus(seed.rarity, seed.isShiny);

        return seed;
    }
}