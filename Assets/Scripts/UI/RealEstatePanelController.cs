using System.Collections.Generic;
using UnityEngine;

public class RealEstatePanelController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public Transform listingParent;
    public GameObject listingPrefab;

    [Header("Warehouse Listings")]
    public List<WarehouseRealEstateData> warehouseListings = new List<WarehouseRealEstateData>();

    [Header("Managers")]
    public EconomyManager economyManager;
    public WarehouseManager warehouseManager;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);

        if (panelRoot.activeSelf)
            RefreshUI();
    }

    public void RefreshUI()
    {
        if (listingParent == null || listingPrefab == null)
        {
            Debug.LogWarning("RealEstatePanelController missing listingParent or listingPrefab.");
            return;
        }

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

        // Spawn each listing
        foreach (WarehouseRealEstateData listing in warehouseListings)
        {
            if (listing == null)
                continue;

            GameObject obj = Instantiate(listingPrefab, listingParent);

            WarehouseListingUI ui = obj.GetComponent<WarehouseListingUI>();
            if (ui == null)
            {
                Debug.LogError("Listing Prefab is missing WarehouseListingUI component.");
                continue;
            }

            bool unlocked = warehouseManager.IsWarehouseUnlocked(listing.warehouseName);
            bool isActive = (warehouseManager.activeWarehouse != null &&
                             warehouseManager.activeWarehouse.warehouseName == listing.warehouseName);

            ui.Setup(listing, unlocked, isActive, this);
        }
    }

    public void OnClickPurchaseOrSelect(WarehouseRealEstateData listing)
    {
        if (listing == null)
            return;

        bool unlocked = warehouseManager.IsWarehouseUnlocked(listing.warehouseName);

        // Already unlocked? Switch to it.
        if (unlocked)
        {
            warehouseManager.SetActiveWarehouse(listing.warehouseName);
            RefreshUI();
            return;
        }

        // Locked: attempt purchase
        if (economyManager.Money < listing.cost)
        {
            Debug.Log($"Not enough money to buy {listing.warehouseName}");
            return;
        }

        bool success = economyManager.SpendMoney(listing.cost);
        if (!success)
            return;

        warehouseManager.UnlockWarehouse(listing.warehouseName);
        warehouseManager.SetActiveWarehouse(listing.warehouseName);

        Debug.Log($"Purchased warehouse: {listing.warehouseName}");

        RefreshUI();
    }
}

[System.Serializable]
public class WarehouseRealEstateData
{
    public string warehouseName = "Warehouse 01";
    public int cost = 10000;

    [TextArea]
    public string description = "A basic warehouse space.";
}