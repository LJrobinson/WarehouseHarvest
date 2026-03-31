using UnityEngine;

public class PlantInstance : MonoBehaviour
{
    [Header("Seed Info")]
    public SeedInstance seed;

    public PlantStrainData strainData => seed != null ? seed.strain : null;

    [Header("Progress")]
    [Range(0f, 100f)]
    public float growthPercent = 0f;

    [Range(0f, 150f)]
    public float ripenessPercent = 0f;

    [Header("Stage")]
    public PlantStage stage = PlantStage.Seed;

    public bool IsHarvestable => stage == PlantStage.Harvestable;
    public bool IsOverripe => stage == PlantStage.Overripe;

    private void Start()
    {
        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (seed == null)
            return;

        Renderer r = GetComponent<Renderer>();
        if (r == null)
            return;

        if (seed.isShiny)
        {
            r.material.color = Color.magenta; // Shiny plants look wild
        }
        else
        {
            // Normal rarity coloring
            r.material.color = seed.rarity switch
            {
                SeedRarity.Common => Color.green,
                SeedRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f),
                SeedRarity.Rare => Color.cyan,
                SeedRarity.Epic => new Color(0.7f, 0.2f, 1f),
                SeedRarity.Legendary => Color.yellow,
                _ => Color.green
            };
        }
    }

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

        // Stage logic
        if (growthPercent < 25f)
            stage = PlantStage.Seed;
        else if (growthPercent < 60f)
            stage = PlantStage.Veg;
        else
            stage = PlantStage.Flower;

        // Ripeness in flower
        if (stage == PlantStage.Flower)
        {
            ripenessPercent += strainData.ripenessPerDayInFlower;
        }

        // Harvestable / Overripe
        if (ripenessPercent >= strainData.harvestWindowStart && ripenessPercent <= strainData.harvestWindowEnd)
            stage = PlantStage.Harvestable;

        if (ripenessPercent >= strainData.overripeThreshold)
            stage = PlantStage.Overripe;

        Debug.Log($"[{strainData.strainName}] ({seed.rarity}) Shiny:{seed.isShiny} Stage:{stage} Growth:{growthPercent}% Ripeness:{ripenessPercent}%");
    }
}