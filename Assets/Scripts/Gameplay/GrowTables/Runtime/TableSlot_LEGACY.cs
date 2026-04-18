using UnityEngine;

public class TableSlot : MonoBehaviour
{
    public PlantInstance currentPlant;

    public bool IsEmpty => currentPlant == null;

    public void PlantSeed(PlantInstance plantPrefab)
    {
        if (!IsEmpty)
        {
            Debug.LogWarning("Slot already occupied!");
            return;
        }

        currentPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        currentPlant.transform.SetParent(transform);

        GrowTable table = GetComponentInParent<GrowTable>();
        if (table != null)
            currentPlant.parentTable = table;
    }

    public void RemovePlant()
    {
        if (currentPlant != null)
        {
            Destroy(currentPlant.gameObject);
            currentPlant = null;
        }
    }
}