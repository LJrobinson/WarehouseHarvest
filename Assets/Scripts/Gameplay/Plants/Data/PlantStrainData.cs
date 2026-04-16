using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantStrain", menuName = "WarehouseHarvest/Plant Strain")]
public class PlantStrainData : ScriptableObject
{
    [Header("Basic Info")]
    public string strainName = "New Strain";

    [TextArea]
    public string description;

    [Header("Care Preferences")]
    public float idealWaterMin = 40f;
    public float idealWaterMax = 70f;

    public float idealNutrientsMin = 40f;
    public float idealNutrientsMax = 75f;

    [Range(0f, 1f)]
    public float moldSusceptibility = 0.25f;

    [Range(0f, 1f)]
    public float pestSusceptibility = 0.20f;

    [Header("Growth Settings")]
    public float growthPerDay = 12f;

    [Header("Ripeness Settings")]
    public float ripenessPerDayInFlower = 10f;

    public float harvestWindowStart = 100f;
    public float harvestWindowEnd = 110f;

    public float overripeThreshold = 120f;

    [Header("Grading / Economy")]
    public int seedCost = 25;

    [Header("Seed Packs")]
    public int pack5Cost = 100;
    public int pack20Cost = 350;

    [Range(0f, 2f)]
    public float payoutMultiplier = 1.0f;

    [Range(0, 250)]
    public int geneticsScore = 250;

    [Header("Rarity / Phenotype")]
    [Range(0f, 0.2f)]
    public float shinyChance = 0.01f;

    [Header("Harvest Product")]
    public float baseYieldGrams = 10f;

    public int baseValuePerGram = 20;

    public float basePricePerGram = 10f;
}