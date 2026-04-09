using UnityEngine;

public class RealEstatePanelController : UIPanel
{
    [Header("UI References")]
    public Transform listingParent;
    public GameObject listingPrefab;

    [Header("Managers")]
    public EconomyManager economyManager;
    public WarehouseManager warehouseManager;

    protected override void OnOpened()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (listingParent == null || listingPrefab == null)
        {
            Debug.LogWarning("RealEstatePanelController missing listingParent or listingPrefab.");
            return;
        }

        if (economyManager == null)
            economyManager = EconomyManager.Instance;

        if (warehouseManager == null)
            warehouseManager = WarehouseManager.Instance;

        if (economyManager == null || warehouseManager == null)
        {
            Debug.LogWarning("RealEstatePanelController missing EconomyManager or WarehouseManager reference.");
            return;
        }

        // Clear old UI objects
        for (int i = listingParent.childCount - 1; i >= 0; i--)
        {
            Destroy(listingParent.GetChild(i).gameObject);
        }

        // Spawn each warehouse from real warehouse system
        foreach (Warehouse warehouse in warehouseManager.allWarehouses)
        {
            if (warehouse == null)
                continue;

            GameObject obj = Instantiate(listingPrefab, listingParent);

            WarehouseListingUI ui = obj.GetComponent<WarehouseListingUI>();
            if (ui == null)
            {
                Debug.LogError("Listing Prefab is missing WarehouseListingUI component.");
                continue;
            }

            bool unlocked = warehouseManager.IsWarehouseUnlocked(warehouse.warehouseName);
            bool isActive = (warehouseManager.activeWarehouse != null &&
                             warehouseManager.activeWarehouse.warehouseName == warehouse.warehouseName);

            ui.Setup(warehouse, unlocked, isActive, this);
        }
    }

    public void OnClickPurchaseOrSelect(Warehouse warehouse)
    {
        if (warehouse == null)
            return;

        bool unlocked = warehouseManager.IsWarehouseUnlocked(warehouse.warehouseName);

        // Already unlocked? Switch to it.
        if (unlocked)
        {
            warehouseManager.SetActiveWarehouse(warehouse.warehouseName);
            RefreshUI();
            return;
        }

        // Locked: attempt purchase
        if (economyManager.Money < warehouse.purchaseCost)
        {
            Debug.Log($"Not enough money to buy {warehouse.warehouseName}");
            return;
        }

        bool success = economyManager.SpendMoney(warehouse.purchaseCost);
        if (!success)
            return;

        warehouseManager.UnlockWarehouse(warehouse.warehouseName);
        warehouseManager.SetActiveWarehouse(warehouse.warehouseName);

        Debug.Log($"Purchased warehouse: {warehouse.warehouseName}");

        RefreshUI();
    }
}