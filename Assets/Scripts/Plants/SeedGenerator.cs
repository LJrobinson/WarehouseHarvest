using UnityEngine;

public static class SeedGenerator
{
    public static SeedInstance GenerateSeed(PlantStrainData strain)
    {
        SeedInstance seed = new SeedInstance();
        seed.strain = strain;

        seed.isMysterySeed = false;

        // Rarity roll (simple default for now)
        float roll = Random.value;

        if (roll < 0.60f) seed.rarity = SeedRarity.Common;
        else if (roll < 0.85f) seed.rarity = SeedRarity.Uncommon;
        else if (roll < 0.95f) seed.rarity = SeedRarity.Rare;
        else if (roll < 0.99f) seed.rarity = SeedRarity.Epic;
        else seed.rarity = SeedRarity.Legendary;

        // Shiny roll (strain based)
        seed.isShiny = Random.value < strain.shinyChance;

        return seed;
    }

    public static SeedInstance GenerateMysterySeed(StrainDatabase db)
    {
        SeedInstance seed = new SeedInstance();

        seed.strain = null;
        seed.isMysterySeed = true;

        // Assign hidden real strain
        seed.hiddenStrain = db.GetRandomStrain();

        // Rarity roll
        float roll = Random.value;

        if (roll < 0.70f) seed.rarity = SeedRarity.Common;
        else if (roll < 0.90f) seed.rarity = SeedRarity.Uncommon;
        else if (roll < 0.97f) seed.rarity = SeedRarity.Rare;
        else if (roll < 0.995f) seed.rarity = SeedRarity.Epic;
        else seed.rarity = SeedRarity.Legendary;

        // Shiny roll
        seed.isShiny = Random.value < 0.01f;

        return seed;
    }
}