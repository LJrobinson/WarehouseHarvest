using UnityEngine;

public class PlantInstance : MonoBehaviour
{
    [Header("Progress")]
    [Range(0f, 100f)]
    public float growthPercent = 0f;

    [Range(0f, 150f)]
    public float ripenessPercent = 0f;

    [Header("Stage")]
    public PlantStage stage = PlantStage.Seed;

    public bool IsHarvestable => stage == PlantStage.Harvestable;
    public bool IsOverripe => stage == PlantStage.Overripe;

    public void AdvanceDay()
    {
        // Growth always increases until 100%
        if (growthPercent < 100f)
        {
            growthPercent = Mathf.Clamp(growthPercent + 12f, 0f, 100f);
        }

        // Stage logic based on growth
        if (growthPercent < 25f)
            stage = PlantStage.Seed;
        else if (growthPercent < 60f)
            stage = PlantStage.Veg;
        else
            stage = PlantStage.Flower;

        // Ripeness only starts in Flower
        if (stage == PlantStage.Flower)
        {
            ripenessPercent += 10f;
        }

        // Harvestable / Overripe thresholds
        if (ripenessPercent >= 100f && ripenessPercent < 120f)
            stage = PlantStage.Harvestable;

        if (ripenessPercent >= 120f)
            stage = PlantStage.Overripe;

        Debug.Log($"Plant advanced day. Stage: {stage}, Growth: {growthPercent}%, Ripeness: {ripenessPercent}%");
    }
}