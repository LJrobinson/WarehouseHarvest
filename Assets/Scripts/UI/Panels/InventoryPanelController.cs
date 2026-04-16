using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelController : MonoBehaviour
{
    [Header("References")]
    public SeedInventory seedInventory;

    [Header("UI List")]
    public Transform contentParent;
    public InventorySeedRowUI rowPrefab;

    [Header("Details Panel")]
    public InventorySeedDetailsUI detailsPanel;

    private readonly List<InventorySeedRowUI> spawnedRows = new();

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (seedInventory == null)
        {
            Debug.LogError("[InventoryPanelController] SeedInventory not assigned.");
            return;
        }

        ClearRows();

        List<SeedInventorySummary> summaries = seedInventory.GetSummaries();

        summaries.Sort((a, b) => b.totalCount.CompareTo(a.totalCount));

        foreach (var summary in summaries)
        {
            InventorySeedRowUI row = Instantiate(rowPrefab, contentParent);
            row.Setup(summary, OnRowClicked);
            spawnedRows.Add(row);
        }

        if (summaries.Count == 0 && detailsPanel != null)
            detailsPanel.Clear();
    }

    private void OnRowClicked(SeedInventorySummary summary)
    {
        if (detailsPanel == null)
            return;

        detailsPanel.Show(summary);
    }

    private void ClearRows()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }

        spawnedRows.Clear();
    }
}