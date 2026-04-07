using System.Collections.Generic;
using UnityEngine;

public class WarehouseManager : MonoBehaviour
{
    public static WarehouseManager Instance;

    [Header("Warehouses in Scene")]
    public List<Warehouse> allWarehouses = new List<Warehouse>();

    [Header("Active Warehouse")]
    public Warehouse activeWarehouse;

    [Header("Unlocked Warehouses")]
    public List<string> unlockedWarehouseNames = new List<string>();

    private void Awake()
    {
        Instance = this;

        // Default unlock first warehouse if none exist
        if (unlockedWarehouseNames.Count == 0 && allWarehouses.Count > 0)
        {
            unlockedWarehouseNames.Add(allWarehouses[0].warehouseName);
        }

        // Default active warehouse
        if (activeWarehouse == null && allWarehouses.Count > 0)
        {
            SetActiveWarehouse(allWarehouses[0].warehouseName);
        }
    }

    public void RegisterWarehouse(Warehouse warehouse)
    {
        if (warehouse == null) return;

        if (!allWarehouses.Contains(warehouse))
        {
            allWarehouses.Add(warehouse);
        }
    }

    public void UnregisterWarehouse(Warehouse warehouse)
    {
        if (warehouse == null) return;

        if (allWarehouses.Contains(warehouse))
        {
            allWarehouses.Remove(warehouse);
        }
    }

    public bool IsWarehouseUnlocked(string warehouseName)
    {
        return unlockedWarehouseNames.Contains(warehouseName);
    }

    public void UnlockWarehouse(string warehouseName)
    {
        if (!unlockedWarehouseNames.Contains(warehouseName))
        {
            unlockedWarehouseNames.Add(warehouseName);
        }
    }

    public void SetActiveWarehouse(string warehouseName)
    {
        Warehouse target = GetWarehouseByName(warehouseName);

        if (target == null)
        {
            Debug.LogWarning($"WarehouseManager: Could not find warehouse named {warehouseName}");
            return;
        }

        foreach (Warehouse wh in allWarehouses)
        {
            if (wh == null) continue;
            wh.gameObject.SetActive(false);
        }

        target.gameObject.SetActive(true);
        activeWarehouse = target;

        Debug.Log($"Active Warehouse set to: {warehouseName}");
    }

    public Warehouse GetWarehouseByName(string warehouseName)
    {
        foreach (Warehouse wh in allWarehouses)
        {
            if (wh == null) continue;

            if (wh.warehouseName == warehouseName)
                return wh;
        }

        return null;
    }
}