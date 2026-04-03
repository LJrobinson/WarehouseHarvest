using System.Collections.Generic;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    public List<GrowTable> tables = new List<GrowTable>();

    private void Awake()
    {
        // Auto-fill tables if not assigned
        if (tables.Count == 0)
        {
            tables.AddRange(GetComponentsInChildren<GrowTable>());
        }

        // Default: first table unlocked
        if (tables.Count > 0)
        {
            tables[0].isUnlocked = true;
        }
    }
}