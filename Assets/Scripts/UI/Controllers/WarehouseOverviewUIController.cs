using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarehouseOverviewUIController : MonoBehaviour
{
    [Header("References")]
    public DevSandboxControllerUI devUI; // so we can select tables
    public Warehouse warehouse;
    public GameObject warehousePanel;

    [Header("Header UI")]
    public TMP_Text warehouseTitleText;
    public TMP_Text warehouseSummaryText;

    [Header("Table List UI")]
    public Transform tableListParent;
    public GameObject tableRowPrefab;

    [Header("Selection UI")]
    public TMP_Text selectedTableText;

    [Header("Utility UI")]
    public TMP_Text utilitiesText;

    [Header("Upgrade Buttons")]
    public Button upgradePowerButton;
    public Button upgradeWaterButton;
    public Button upgradeDataButton;

    public TMP_Dropdown allocationModeDropdown;

    private GrowTable selectedTable;

    private bool buttonsHooked = false;

    private void Start()
    {
        if (warehouse != null)
            SetWarehouse(warehouse);
        else
            Debug.LogWarning("WarehouseOverviewUIController: No warehouse assigned in inspector!");
    }

    private void HookButtons()
    {
        if (buttonsHooked) return;

        if (upgradeDataButton != null)
            upgradeDataButton.onClick.AddListener(() =>
            {
                Debug.Log("Upgrade Data Clicked!");
                if (warehouse == null) return;
                warehouse.UpgradeData();
                RefreshUI();
            });

        if (upgradePowerButton != null)
            upgradePowerButton.onClick.AddListener(() =>
            {
                Debug.Log("Upgrade Power Clicked!");
                if (warehouse == null) return;
                warehouse.UpgradePower();
                RefreshUI();
            });

        if (upgradeWaterButton != null)
            upgradeWaterButton.onClick.AddListener(() =>
            {
                Debug.Log("Upgrade Water Clicked!");
                if (warehouse == null) return;
                warehouse.UpgradeWater();
                RefreshUI();
            });     

        buttonsHooked = true;
    }

    public void SetWarehouse(Warehouse newWarehouse)
    {
        Debug.Log("SetWarehouse CALLED");

        // Assign warehouse first
        warehouse = newWarehouse;

        if (warehouse == null)
        {
            Debug.LogWarning("WarehouseOverviewUIController: SetWarehouse called with null warehouse!");
            return;
        }


        selectedTable = null;

        // ---- Setup Dropdown ----
        if (allocationModeDropdown != null)
        {
            // Clear old listeners/options
            allocationModeDropdown.onValueChanged.RemoveAllListeners();
            allocationModeDropdown.ClearOptions();

            // Add options
            List<string> options = new List<string>
        {
            "Manual Priority",
            "Balanced",
            "Plants First",
            "Harvest First",
            "High Value First"
        };
            allocationModeDropdown.AddOptions(options);

            // Set value to current allocation mode
            allocationModeDropdown.value = (int)warehouse.allocationMode;

            // Add listener
            allocationModeDropdown.onValueChanged.AddListener((val) =>
            {
                if (warehouse == null) return;

                if (val < 0 || val >= options.Count)
                {
                    Debug.LogWarning("Invalid allocation mode index: " + val);
                    return;
                }

                warehouse.allocationMode = (WarehouseAllocationMode)val;
                warehouse.UpdateUtilityStatusForTables();
                RefreshUI();
            });
        }
        else
        {
            Debug.LogWarning("WarehouseOverviewUIController: allocationModeDropdown not assigned!");
        }

        // ---- Hook Upgrade Buttons ----
        HookButtons();

        // ---- Refresh UI ----
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (warehouse == null)
        {
            Debug.LogWarning("WarehouseOverviewUIController: warehouse is null.");
            return;
        }

        RefreshHeader();
        RefreshTableList();
    }

    private void RefreshHeader()
    {
        float dataUse = warehouse.GetTotalDataUsage();
        float powerUse = warehouse.GetTotalPowerUsage();
        float waterUse = warehouse.GetTotalWaterUsage();
        
        string utilityWarning = warehouse.HasEnoughUtilities() ? "" : "\n<color=red>UTILITIES OVER CAPACITY</color>";

        if (utilitiesText != null)
        {
            utilitiesText.text =
                $"Data:  {dataUse}/{warehouse.maxData} (Lvl {warehouse.dataLevel})\n" +
                $"Power: {powerUse}/{warehouse.maxPower} (Lvl {warehouse.powerLevel})\n" +
                $"Water: {waterUse}/{warehouse.maxWater} (Lvl {warehouse.waterLevel})" +
                utilityWarning;
        }

        if (warehouseTitleText != null)
            warehouseTitleText.text = $"{warehouse.name}:\n AKA: {warehouse.warehouseName}";

        int unlockedTables = 0;
        int totalPlants = 0;
        int harvestablePlants = 0;
        int emptySlots = 0;

        foreach (GrowTable table in warehouse.tables)
        {
            if (table == null)
                continue;

            if (table.isUnlocked)
                unlockedTables++;

            for (int i = 0; i < table.unlockedSlots; i++)
            {
                if (i >= table.slots.Count)
                    continue;

                TableSlot slot = table.slots[i];

                if (slot == null)
                    continue;

                if (slot.IsEmpty)
                {
                    emptySlots++;
                }
                else
                {
                    totalPlants++;

                    PlantInstance plant = slot.currentPlant;
                    if (plant != null && plant.IsHarvestable)
                        harvestablePlants++;
                }
            }
        }

        if (warehouseSummaryText != null)
        {
            warehouseSummaryText.text =
                $"Tables Unlocked: {unlockedTables}/{warehouse.tables.Count}\n" +
                $"Plants Growing: {totalPlants}\n" +
                $"Harvestable: {harvestablePlants}\n" +
                $"Empty Slots: {emptySlots}";
        }

        if (selectedTableText != null)
        {
            if (selectedTable == null)
                selectedTableText.text = "Selected Table: None";
            else
                selectedTableText.text = $"Selected Table: {selectedTable.name}";
        }
    }

    private void RefreshTableList()
    {
        if (tableListParent == null || tableRowPrefab == null)
        {
            Debug.LogError("WarehouseOverviewUIController: tableListParent or tableRowPrefab not assigned.");
            return;
        }

        foreach (Transform child in tableListParent)
            Destroy(child.gameObject);

        for (int i = 0; i < warehouse.tables.Count; i++)
        {
            GrowTable table = warehouse.tables[i];
            if (table == null)
                continue;

            int plantsGrowing = 0;
            int harvestable = 0;
            int empty = 0;

            for (int s = 0; s < table.unlockedSlots; s++)
            {
                if (s >= table.slots.Count)
                    continue;

                TableSlot slot = table.slots[s];

                if (slot == null)
                    continue;

                if (slot.IsEmpty)
                {
                    empty++;
                }
                else
                {
                    plantsGrowing++;

                    if (slot.currentPlant != null && slot.currentPlant.IsHarvestable)
                        harvestable++;
                }
            }

            GameObject rowObj = Instantiate(tableRowPrefab, tableListParent);

            TMP_Text txt = rowObj.GetComponentInChildren<TMP_Text>();
            Button btn = rowObj.GetComponent<Button>();

            string lockedText = table.isUnlocked ? "" : "LOCKED | ";
            string offlineText = (table.isUnlocked && !table.utilitiesOnline) ? "<color=red>OFFLINE</color> | " : "";

            if (txt != null)
            {
                txt.text =
                            $"{lockedText}{offlineText}{table.name} | Slots {table.unlockedSlots}/{table.slots.Count} | " +
                            $"Plants {plantsGrowing} | Harvestable {harvestable} | Empty {empty}";
            }

            if (btn != null)
            {
                int capturedIndex = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    SelectTable(capturedIndex);
                });

                // allow clicking locked tables too (so player can inspect them)
                btn.interactable = true;
            }
        }
    }

    private void SelectTable(int index)
    {
        if (index < 0 || index >= warehouse.tables.Count)
            return;

        selectedTable = warehouse.tables[index];

        Debug.Log($"WarehouseOverviewUI: Selected table {selectedTable.name}");

        // Tell DevSandboxControllerUI to switch to this table if you want:
        if (devUI != null)
        {
            devUI.ForceSelectTable(selectedTable);
        }

        RefreshHeader();
    }

    public void TogglePanel()
    {
        bool nowOpen = !warehousePanel.activeSelf;
        warehousePanel.SetActive(nowOpen);

        if (nowOpen)
            RefreshUI();
    }
    
}