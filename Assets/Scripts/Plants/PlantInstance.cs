using UnityEngine;

public class PlantInstance : MonoBehaviour
{
    [Header("Seed Info")]
    public SeedInstance seed;

    public PlantStrainData strainData => seed != null ? seed.strain : null;

    [Header("Care Stats")]
    [Range(0f, 100f)] public float waterLevel = 60f;
    [Range(0f, 100f)] public float nutrientsLevel = 60f;
    [Range(0f, 100f)] public float stress = 0f;
    [Range(0f, 100f)] public float health = 100f;

    public bool hasMold = false;
    public bool hasPests = false;

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

    public void AdvanceDay(float lightMultiplier)
    {
        if (strainData == null)
        {
            Debug.LogError("PlantInstance has no strainData assigned!");
            return;
        }

        // Growth
        if (growthPercent < 100f)
        {
            growthPercent = Mathf.Clamp(growthPercent + (strainData.growthPerDay * lightMultiplier), 0f, 100f);
        }

        // Stage logic
        if (growthPercent < 25f)
            stage = PlantStage.Seed;
        else if (growthPercent < 60f)
            stage = PlantStage.Veg;
        else
            stage = PlantStage.Flower;

        // Drain daily resources
        waterLevel = Mathf.Clamp(waterLevel - 12f, 0f, 100f);

        if (stage == PlantStage.Veg || stage == PlantStage.Flower)
            nutrientsLevel = Mathf.Clamp(nutrientsLevel - 8f, 0f, 100f);

        // Stress calculation based on ideal ranges
        bool waterOk = waterLevel >= strainData.idealWaterMin && waterLevel <= strainData.idealWaterMax;
        bool nutrientsOk = nutrientsLevel >= strainData.idealNutrientsMin && nutrientsLevel <= strainData.idealNutrientsMax;

        if (!waterOk)
            stress += 6f;
        else
            stress -= 2f;

        if (stage == PlantStage.Veg || stage == PlantStage.Flower)
        {
            if (!nutrientsOk)
                stress += 4f;
            else
                stress -= 1f;
        }

        stress = Mathf.Clamp(stress, 0f, 100f);

        // Health damage if stress is high
        if (stress > 70f)
            health -= 4f;

        if (waterLevel <= 5f)
            health -= 6f;

        health = Mathf.Clamp(health, 0f, 100f);

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

        float baseMoldChance = 0.01f + (stress / 100f) * 0.05f;
        float basePestChance = 0.01f + (stress / 100f) * 0.04f;

        // Overwatering increases mold chance
        if (waterLevel > strainData.idealWaterMax)
            baseMoldChance += 0.05f;

        baseMoldChance *= strainData.moldSusceptibility;
        basePestChance *= strainData.pestSusceptibility;

        if (!hasMold && Random.value < baseMoldChance)
        {
            hasMold = true;
            Debug.Log($"MOLD EVENT on {strainData.strainName}!");
        }

        if (!hasPests && Random.value < basePestChance)
        {
            hasPests = true;
            Debug.Log($"PEST EVENT on {strainData.strainName}!");
        }

        // Ongoing damage if infected
        if (hasMold)
        {
            health -= 5f;
            stress += 5f;
        }

        if (hasPests)
        {
            health -= 3f;
            stress += 3f;
        }

        health = Mathf.Clamp(health, 0f, 100f);
        stress = Mathf.Clamp(stress, 0f, 100f);

        Debug.Log($"[{strainData.strainName}] ({seed.rarity}) Shiny:{seed.isShiny} Stage:{stage} Growth:{growthPercent}% Ripeness:{ripenessPercent}%");
    }

    public void WaterPlant(float amount, float stressReduction)
    {
        waterLevel = Mathf.Clamp(waterLevel + amount, 0f, 100f);
        stress = Mathf.Clamp(stress - stressReduction, 0f, 100f);
    }

    public void FeedNutrients(float amount)
    {
        nutrientsLevel = Mathf.Clamp(nutrientsLevel + amount, 0f, 100f);
    }

    public bool TreatMold()
    {
        if (!hasMold) return false;
        hasMold = false;
        stress = Mathf.Clamp(stress - 10f, 0f, 100f);
        return true;
    }

    public bool TreatPests()
    {
        if (!hasPests) return false;
        hasPests = false;
        stress = Mathf.Clamp(stress - 8f, 0f, 100f);
        return true;
    }
}