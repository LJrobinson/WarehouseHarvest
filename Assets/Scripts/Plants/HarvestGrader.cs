using UnityEngine;

public static class HarvestGrader
{
    public static int CalculateScore(PlantInstance plant)
    {
        if (plant == null || plant.strainData == null)
            return 0;

        int score = 0;

        // Genetics (0-250)
        int geneticsScore = Mathf.Clamp(plant.strainData.geneticsScore, 0, 250);

        // Growth score (0-250)
        int growthScore = Mathf.RoundToInt((plant.growthPercent / 100f) * 250f);

        // Ripeness score (0-300)
        int ripenessScore = CalculateRipenessScore(
            plant.ripenessPercent,
            plant.strainData.harvestWindowStart,
            plant.strainData.harvestWindowEnd,
            plant.strainData.overripeThreshold
        );

        // Health score placeholder (0-200)
        int healthScore = 200;

        score = geneticsScore + growthScore + ripenessScore + healthScore;

        // Randomness
        score += Random.Range(-20, 20);

        return Mathf.Clamp(score, 0, 1000);
    }

    private static int CalculateRipenessScore(float ripeness, float windowStart, float windowEnd, float overripeThreshold)
    {
        // Perfect window = 300 points
        if (ripeness >= windowStart && ripeness <= windowEnd)
            return 300;

        // Too early
        if (ripeness < windowStart)
        {
            float percent = ripeness / windowStart; // 0-1
            return Mathf.RoundToInt(percent * 250f);
        }

        // Late but not dead
        if (ripeness > windowEnd && ripeness < overripeThreshold)
            return 200;

        // Overripe penalty
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