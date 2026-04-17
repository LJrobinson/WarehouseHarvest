using UnityEngine;

public class PlantInstance : MonoBehaviour
{
    [Header("Seed Info")]
    public SeedInstance seed;

    [HideInInspector] public GrowTable parentTable;

    public PlantStrainData strainData => seed != null ? seed.EffectiveStrain : null;

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

    [Header("Visual Feedback")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string emissionColorProperty = "_EmissionColor";
    [SerializeField] private float harvestGlowMultiplier = 2f;
    [SerializeField] private float overripeGlowMultiplier = 3f;
    [SerializeField] private bool enableGlowDebugLogs = true;

    private Material runtimeMaterial;
    private Color baseEmissionColor = Color.black;

    private void Start()
    {
        InitializeVisualReferences();
        ApplyVisuals();
        RefreshVisualState();
    }

    private void InitializeVisualReferences()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name}: No targetRenderer found for PlantInstance.");
            return;
        }

        runtimeMaterial = targetRenderer.material;

        if (runtimeMaterial == null)
        {
            Debug.LogWarning($"{name}: targetRenderer exists but has no material.");
            return;
        }

        if (!runtimeMaterial.HasProperty(emissionColorProperty))
        {
            Debug.LogWarning($"{name}: Material '{runtimeMaterial.name}' does not have emission property '{emissionColorProperty}'.");
            return;
        }

        baseEmissionColor = runtimeMaterial.GetColor(emissionColorProperty);

        if (enableGlowDebugLogs)
        {
            Debug.Log(
                $"{name}: Visuals initialized | Renderer:{targetRenderer.name} | Material:{runtimeMaterial.name} | EmissionProperty:{emissionColorProperty}"
            );
        }
    }

    public void ApplyVisuals()
    {
        if (seed == null)
            return;

        if (runtimeMaterial == null)
            return;

        if (seed.isShiny)
        {
            runtimeMaterial.color = Color.magenta;
        }
        else
        {
            runtimeMaterial.color = seed.rarity switch
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

    private void RefreshVisualState()
    {
        if (runtimeMaterial == null)
            return;

        if (!runtimeMaterial.HasProperty(emissionColorProperty))
            return;

        runtimeMaterial.EnableKeyword("_EMISSION");

        if (IsOverripe)
        {
            runtimeMaterial.SetColor(emissionColorProperty, Color.red * overripeGlowMultiplier);

            if (enableGlowDebugLogs)
            {
                Debug.Log(
                    $"{name} | Glow refresh | Stage:{stage} | Harvestable:{IsHarvestable} | Overripe:{IsOverripe} | Emission:RED x{overripeGlowMultiplier}"
                );
            }
        }
        else if (IsHarvestable)
        {
            runtimeMaterial.SetColor(emissionColorProperty, Color.green * harvestGlowMultiplier);

            if (enableGlowDebugLogs)
            {
                Debug.Log(
                    $"{name} | Glow refresh | Stage:{stage} | Harvestable:{IsHarvestable} | Overripe:{IsOverripe} | Emission:GREEN x{harvestGlowMultiplier}"
                );
            }
        }
        else
        {
            runtimeMaterial.SetColor(emissionColorProperty, baseEmissionColor);

            if (enableGlowDebugLogs)
            {
                Debug.Log(
                    $"{name} | Glow refresh | Stage:{stage} | Harvestable:{IsHarvestable} | Overripe:{IsOverripe} | Emission:BASE"
                );
            }
        }
    }

    public void AdvanceDay(float lightMultiplier, float waterQualityBonus)
    {
        if (strainData == null)
        {
            Debug.LogError("PlantInstance has no strainData assigned!");
            return;
        }

        // Utility check
        if (parentTable != null && !parentTable.utilitiesOnline)
        {
            stress += 3f;
            health -= 1f;
            RefreshVisualState();
            return;
        }

        // Growth
        growthPercent = Mathf.Clamp(growthPercent + (strainData.growthPerDay * lightMultiplier), 0f, 100f);

        // Stage progression
        if (growthPercent < 20f)
            stage = PlantStage.Seed;
        else if (growthPercent < 70f)
            stage = PlantStage.Veg;
        else
            stage = PlantStage.Flower;

        // Ripeness only when fully grown
        if (growthPercent >= 100f)
        {
            ripenessPercent = Mathf.Clamp(
                ripenessPercent + (strainData.ripenessPerDayInFlower * lightMultiplier),
                0f,
                150f
            );
        }

        // Harvest / Overripe logic
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

        // Stress checks
        bool waterOk = waterLevel >= strainData.idealWaterMin && waterLevel <= strainData.idealWaterMax;
        bool nutrientsOk = nutrientsLevel >= strainData.idealNutrientsMin && nutrientsLevel <= strainData.idealNutrientsMax;

        if (!waterOk)
            stress += Mathf.Max(0f, 6f - waterQualityBonus);
        else
            stress -= 2f;

        if (!nutrientsOk)
            stress += 4f;
        else
            stress -= 1f;

        stress = Mathf.Clamp(stress, 0f, 100f);

        // Health penalties
        if (stress > 70f) health -= 4f;
        if (waterLevel <= 5f) health -= 6f;

        // Mold / pest chance
        float moldChance = (0.01f + (stress / 100f) * 0.05f) * strainData.moldSusceptibility;
        float pestChance = (0.01f + (stress / 100f) * 0.04f) * strainData.pestSusceptibility;

        if (waterLevel > strainData.idealWaterMax)
            moldChance += 0.05f;

        if (!hasMold && Random.value < moldChance) hasMold = true;
        if (!hasPests && Random.value < pestChance) hasPests = true;

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

        // Final clamp
        health = Mathf.Clamp(health, 0f, 100f);
        stress = Mathf.Clamp(stress, 0f, 100f);

        RefreshVisualState();

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

        if (runtimeMaterial == null)
            InitializeVisualReferences();

        ApplyVisuals();
        RefreshVisualState();
    }
}