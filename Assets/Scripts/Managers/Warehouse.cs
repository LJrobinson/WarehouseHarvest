using System.Collections.Generic;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    public List<GrowTable> tables = new List<GrowTable>();

    private void Awake()
    {
        AutoRegisterTables();
    }

    [ContextMenu("Auto Register Tables")]
    public void AutoRegisterTables()
    {
        tables.Clear();

        GrowTable[] found = GetComponentsInChildren<GrowTable>(true);
        tables.AddRange(found);

        Debug.Log($"[{name}] Registered {tables.Count} tables.");
    }
}