using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GrowEquipmentManager equipmentManager;

    private PlantInstance currentPlant;

    public PlantInstance CurrentPlant => currentPlant;
    public bool HasPlant => currentPlant != null;

    public void SpawnPlantFromSeed(SeedInstance seed)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("PlantManager spawnPoint is not assigned!");
            return;
        }

        if (seed == null || seed.strain == null)
        {
            Debug.LogError("SpawnPlantFromSeed failed: seed is null!");
            return;
        }

        if (currentPlant != null)
        {
            Debug.LogWarning("A plant already exists.");
            return;
        }

        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plant.name = seed.DisplayName;
        plant.transform.position = spawnPoint.position;

        currentPlant = plant.AddComponent<PlantInstance>();
        currentPlant.seed = seed;

        Debug.Log($"Spawned plant from seed: {seed.DisplayName}");
    }

    public void AdvanceDayForPlant()
    {
        if (currentPlant == null)
            return;


        float lightMultiplier = equipmentManager != null ? equipmentManager.GetLightGrowthBonus() : 1f;
        currentPlant.AdvanceDay(lightMultiplier);
    }

    public void DestroyCurrentPlant()
    {
        if (currentPlant == null)
            return;

        Destroy(currentPlant.gameObject);
        currentPlant = null;
    }
}