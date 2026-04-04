using UnityEngine;

public static class HarvestGrader
{
    public static int CalculateScore(PlantInstance plant)
    {
        if (plant == null || plant.strainData == null || plant.seed == null)
            return 0;

        int score = 0;

        int carePenalty = Mathf.RoundToInt((plant.stress / 100f) * 250f);
        int healthPenalty = Mathf.RoundToInt(((100f - plant.health) / 100f) * 250f);

        score -= carePenalty;
        score -= healthPenalty;

        if (plant.hasMold)
            score -= 200;

        if (plant.hasPests)
            score -= 120;

        // Genetics base from strain + rarity bonus
        int geneticsScore = plant.strainData.geneticsScore + plant.seed.geneticsBonus;
        geneticsScore = Mathf.Clamp(geneticsScore, 0, 300);

        // Growth score (0-250)
        int growthScore = Mathf.RoundToInt((plant.growthPercent / 100f) * 250f);

        // Ripeness score (0-300)
        int ripenessScore = CalculateRipenessScore(
            plant.ripenessPercent,
            plant.strainData.harvestWindowStart,
            plant.strainData.harvestWindowEnd,
            plant.strainData.overripeThreshold
        );

        // Health placeholder (0-200)
        int healthScore = 200;

        score = geneticsScore + growthScore + ripenessScore + healthScore;

        // Randomness
        score += Random.Range(-20, 20);

        return Mathf.Clamp(score, 0, 1000);
    }

    private static int CalculateRipenessScore(float ripeness, float windowStart, float windowEnd, float overripeThreshold)
    {
        if (ripeness >= windowStart && ripeness <= windowEnd)
            return 300;

        if (ripeness < windowStart)
        {
            float percent = ripeness / windowStart;
            return Mathf.RoundToInt(percent * 250f);
        }

        if (ripeness > windowEnd && ripeness < overripeThreshold)
            return 200;

        return 80;
    }

    public static string GetGradeLetter(int score)
    {
        if (score >= 950) return "S";
        if (score >= 850) return "A";
        if (score >= 700) return "B";
        if (score >= 500) return "C";
        return "D";
    }
}