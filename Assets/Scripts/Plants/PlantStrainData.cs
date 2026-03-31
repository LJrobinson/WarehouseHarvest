using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantStrain", menuName = "WarehouseHarvest/Plant Strain")]
public class PlantStrainData : ScriptableObject
{
    [Header("Basic Info")]
    public string strainName = "New Strain";

    [TextArea]
    public string description;

    [Header("Growth Settings")]
    public float growthPerDay = 12f;

    [Header("Ripeness Settings")]
    public float ripenessPerDayInFlower = 10f;

    public float harvestWindowStart = 100f;
    public float harvestWindowEnd = 110f;

    public float overripeThreshold = 120f;

    [Header("Grading / Economy")]
    public int seedCost = 25;

    [Range(0f, 2f)]
    public float payoutMultiplier = 1.0f;

    [Range(0, 250)]
    public int geneticsScore = 250;
}