using UnityEngine;

public class PlantInstance : MonoBehaviour
{
    [Header("Strain Data")]
    public PlantStrainData strainData;

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
        if (strainData == null)
        {
            Debug.LogError("PlantInstance has no strainData assigned!");
            return;
        }

        // Growth
        if (growthPercent < 100f)
        {
            growthPercent = Mathf.Clamp(growthPercent + strainData.growthPerDay, 0f, 100f);
        }

        // Stage based on growth %
        if (growthPercent < 25f)
            stage = PlantStage.Seed;
        else if (growthPercent < 60f)
            stage = PlantStage.Veg;
        else
            stage = PlantStage.Flower;

        // Ripeness only builds in flower
        if (stage == PlantStage.Flower)
        {
            ripenessPercent += strainData.ripenessPerDayInFlower;
        }

        // Harvestable logic
        if (ripenessPercent >= strainData.harvestWindowStart && ripenessPercent <= strainData.harvestWindowEnd)
        {
            stage = PlantStage.Harvestable;
        }

        // Overripe logic
        if (ripenessPercent >= strainData.overripeThreshold)
        {
            stage = PlantStage.Overripe;
        }

        Debug.Log($"[{strainData.strainName}] Stage: {stage}, Growth: {growthPercent}%, Ripeness: {ripenessPercent}%");
    }
}