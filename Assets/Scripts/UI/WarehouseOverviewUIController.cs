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

    private GrowTable selectedTable;

    public void SetWarehouse(Warehouse newWarehouse)
    {
        warehouse = newWarehouse;
        selectedTable = null;
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
        if (warehouseTitleText != null)
            warehouseTitleText.text = $"WAREHOUSE: {warehouse.name}";

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

            if (txt != null)
            {
                txt.text =
                    $"{lockedText}{table.name} | Slots {table.unlockedSlots}/{table.slots.Count} | " +
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
        if (warehousePanel != null)
            warehousePanel.SetActive(!warehousePanel.activeSelf);
        RefreshUI();
    }
    
}