using System.Collections.Generic;
using UnityEngine;

public class SeedInventoryUI : MonoBehaviour
{
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private DevSandboxUI sandboxUI;

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject rowPrefab;

    private List<GameObject> spawnedRows = new List<GameObject>();

    public void Refresh()
    {
       

        // Clear old rows
        foreach (GameObject row in spawnedRows)
        {
            Destroy(row);
        }
        spawnedRows.Clear();

        List<SeedInstance> seeds = seedInventory.GetAllSeeds();

        foreach (SeedInstance seed in seeds)
        {
            GameObject rowObj = Instantiate(rowPrefab, contentParent);
            SeedRowUI rowUI = rowObj.GetComponent<SeedRowUI>();

            if (rowUI == null)
            {
                Debug.LogError($"Row prefab {rowPrefab.name} is missing SeedRowUI!");
                continue;
            }

            rowUI.Setup(seed, sandboxUI);

            spawnedRows.Add(rowObj);
        }
    }
}