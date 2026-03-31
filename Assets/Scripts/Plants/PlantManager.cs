using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    public void SpawnPlaceholderPlant()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("PlantManager spawnPoint is not assigned!");
            return;
        }

        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plant.name = "PlaceholderPlant";

        plant.transform.position = spawnPoint.position;

        Debug.Log("Spawned PlaceholderPlant cube.");
    }
}