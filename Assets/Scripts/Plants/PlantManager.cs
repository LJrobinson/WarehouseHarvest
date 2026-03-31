using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private PlantInstance currentPlant;

    public PlantInstance CurrentPlant => currentPlant;

    public void SpawnPlaceholderPlant()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("PlantManager spawnPoint is not assigned!");
            return;
        }

        if (currentPlant != null)
        {
            Debug.LogWarning("A plant already exists. Cannot spawn another yet.");
            return;
        }

        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plant.name = "PlaceholderPlant";
        plant.transform.position = spawnPoint.position;

        currentPlant = plant.AddComponent<PlantInstance>();

        Debug.Log("Spawned PlaceholderPlant cube with PlantInstance.");
    }

    public void AdvanceDayForPlant()
    {
        if (currentPlant == null)
        {
            Debug.LogWarning("No plant exists to advance.");
            return;
        }

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