using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private PlantInstance currentPlant;

    public PlantInstance CurrentPlant => currentPlant;

    public bool HasPlant => currentPlant != null;

    public void SpawnPlant(PlantStrainData strain)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("PlantManager spawnPoint is not assigned!");
            return;
        }

        if (strain == null)
        {
            Debug.LogError("SpawnPlant failed: strain is null!");
            return;
        }

        if (currentPlant != null)
        {
            Debug.LogWarning("A plant already exists. Cannot spawn another yet.");
            return;
        }

        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plant.name = strain.strainName;
        plant.transform.position = spawnPoint.position;

        currentPlant = plant.AddComponent<PlantInstance>();
        currentPlant.strainData = strain;

        Debug.Log($"Spawned plant strain: {strain.strainName}");
    }

    public void AdvanceDayForPlant()
    {
        if (currentPlant == null)
            return;

        currentPlant.AdvanceDay();
    }

    public void DestroyCurrentPlant()
    {
        if (currentPlant == null)
            return;

        Destroy(currentPlant.gameObject);
        currentPlant = null;
    }
}