using UnityEngine;

public static class HarvestProcessor
{
    public static HarvestProductInstance CreateHarvestProduct(PlantInstance plant)
    {
        if (plant == null || plant.strainData == null || plant.seed == null)
            return null;

        int score = HarvestGrader.CalculateScore(plant);
        string grade = HarvestGrader.GetGradeLetter(score);

        // Yield is based on strain base yield + score scaling
        float baseGrams = plant.strainData.baseYieldGrams;

        // Score affects yield from 50% to 130%
        float scoreMultiplier = Mathf.Lerp(0.5f, 1.3f, score / 1000f);

        float grams = baseGrams * scoreMultiplier;

        // Rarity yield bonus
        grams += plant.seed.rarity switch
        {
            SeedRarity.Common => 0f,
            SeedRarity.Uncommon => 0.5f,
            SeedRarity.Rare => 1.5f,
            SeedRarity.Epic => 3f,
            SeedRarity.Legendary => 6f,
            _ => 0f
        };

        // Shiny bonus yield
        if (plant.seed.isShiny)
            grams *= 1.25f;

        grams = Mathf.Max(0.1f, grams);

        int baseValuePerGram = plant.strainData.baseValuePerGram;

        // Apply payout multiplier from strain
        baseValuePerGram = Mathf.RoundToInt(baseValuePerGram * plant.strainData.payoutMultiplier);

        return new HarvestProductInstance()
        {
            strainName = plant.strainData.strainName,
            rarity = plant.seed.rarity,
            isShiny = plant.seed.isShiny,

            gradeScore = score,
            gradeLetter = grade,

            grams = grams,
            baseValuePerGram = baseValuePerGram
        };
    }
}