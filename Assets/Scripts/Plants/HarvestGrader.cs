using UnityEngine;

public static class HarvestGrader
{
    public static int CalculateScore(PlantInstance plant)
    {
        if (plant == null)
            return 0;

        // Basic placeholder grading:
        // If fully grown, it gets a decent score.
        // Later we'll add ripeness window, watering, nutrients, etc.
        float growth = plant.growthPercent;

        int baseScore = Mathf.RoundToInt(growth * 10f); // 0-1000

        // Add slight randomness to make it feel like "real grading"
        int randomBonus = Random.Range(-25, 25);

        int finalScore = Mathf.Clamp(baseScore + randomBonus, 0, 1000);

        return finalScore;
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