using UnityEngine;

public static class HarvestGrader
{
    public static int CalculateScore(PlantInstance plant)
    {
        if (plant == null)
            return 0;

        int score = 0;

        // Genetics placeholder (later will come from ScriptableObject)
        int geneticsScore = 250;

        // Growth score (0-250)
        int growthScore = Mathf.RoundToInt((plant.growthPercent / 100f) * 250f);

        // Ripeness timing score (0-300)
        int ripenessScore = CalculateRipenessScore(plant.ripenessPercent);

        // Health score placeholder (later will use pests/mold)
        int healthScore = 200;

        score = geneticsScore + growthScore + ripenessScore + healthScore;

        // Small randomness to simulate real-world variation
        score += Random.Range(-20, 20);

        return Mathf.Clamp(score, 0, 1000);
    }

    private static int CalculateRipenessScore(float ripenessPercent)
    {
        // Perfect harvest window: 100% - 110%
        // Early: <100
        // Late: >110

        if (ripenessPercent < 80f)
            return 50; // way too early

        if (ripenessPercent < 100f)
            return 150; // early but close

        if (ripenessPercent <= 110f)
            return 300; // PERFECT WINDOW

        if (ripenessPercent <= 120f)
            return 200; // late

        return 80; // overripe
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