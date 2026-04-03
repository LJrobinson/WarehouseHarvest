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

    public void ApplyVisuals()
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

    public void AdvanceDay(float lightMultiplier, float waterQualityBonus)
    {
        if (strainData == null)
        {
            Debug.LogError("PlantInstance has no strainData assigned!");
            return;
        }

        // Growth
        growthPercent = Mathf.Clamp(growthPercent + (strainData.growthPerDay * lightMultiplier), 0f, 100f);

        // ============================
        // STAGE PROGRESSION
        // ============================

        // Seed -> Veg -> Flower progression based on growthPercent
        if (growthPercent < 20f)
        {
            stage = PlantStage.Seed;
        }
        else if (growthPercent < 70f)
        {
            stage = PlantStage.Veg;
        }
        else if (growthPercent < 100f)
        {
            stage = PlantStage.Flower;
        }
        else
        {
            // Growth is complete, plant is in flower stage fully
            stage = PlantStage.Flower;
        }

        // ============================
        // RIPENESS PROGRESSION
        // ============================

        // Only build ripeness once fully grown
        if (growthPercent >= 100f)
        {
            ripenessPercent += strainData.ripenessPerDayInFlower * lightMultiplier;
            ripenessPercent = Mathf.Clamp(ripenessPercent, 0f, 150f);
        }

        // Harvest window logic
        if (ripenessPercent >= strainData.harvestWindowStart &&
            ripenessPercent <= strainData.harvestWindowEnd)
        {
            stage = PlantStage.Harvestable;
        }
        else if (ripenessPercent > strainData.overripeThreshold)
        {
            stage = PlantStage.Overripe;
        }

        // Drain resources
        waterLevel = Mathf.Clamp(waterLevel - 12f, 0f, 100f);
        nutrientsLevel = Mathf.Clamp(nutrientsLevel - 8f, 0f, 100f);

        // Stress
        bool waterOk = waterLevel >= strainData.idealWaterMin && waterLevel <= strainData.idealWaterMax;
        bool nutrientsOk = nutrientsLevel >= strainData.idealNutrientsMin && nutrientsLevel <= strainData.idealNutrientsMax;

        stress += !waterOk ? 6f - waterQualityBonus : -2f;
        stress += !nutrientsOk ? 4f : -1f;
        stress = Mathf.Clamp(stress, 0f, 100f);

        // Health penalties
        if (stress > 70f) health -= 4f;
        if (waterLevel <= 5f) health -= 6f;
        health = Mathf.Clamp(health, 0f, 100f);

        // Mold / Pest random events
        float baseMoldChance = 0.01f + (stress / 100f) * 0.05f;
        float basePestChance = 0.01f + (stress / 100f) * 0.04f;

        if (waterLevel > strainData.idealWaterMax) baseMoldChance += 0.05f;

        baseMoldChance *= strainData.moldSusceptibility;
        basePestChance *= strainData.pestSusceptibility;

        if (!hasMold && Random.value < baseMoldChance) hasMold = true;
        if (!hasPests && Random.value < basePestChance) hasPests = true;

        if (hasMold) { health -= 5f; stress += 5f; }
        if (hasPests) { health -= 3f; stress += 3f; }

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

    public void InitializeFromSeed(SeedInstance newSeed)
    {
        seed = newSeed;

        growthPercent = 0f;
        ripenessPercent = 0f;

        waterLevel = 60f;
        nutrientsLevel = 60f;
        stress = 0f;
        health = 100f;

        hasMold = false;
        hasPests = false;

        stage = PlantStage.Seed;

        ApplyVisuals();
    }
}