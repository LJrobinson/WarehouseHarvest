using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevSandboxUI : MonoBehaviour
{
    [Header("Inventory")]
    public SeedInventory seedInventory;
    public Transform seedPanelParent;
    public GameObject seedButtonPrefab;
    public StrainDatabase strainDatabase;
    public EconomyManager economyManager;

    [Header("Tables")]
    public PlantManager plantManager;
    public GrowTable selectedTable;

    [Header("Prefabs")]
    public PlantInstance plantPrefab; // normal plant prefab

    [Header("UI")]
    public TMP_Text selectedSeedText;
    public TMP_Text selectedTableText;
    public TMP_Text plantStatsText;
    public TMP_InputField strainSearchInput;
    public TMP_Dropdown strainDropdown;

    private SeedInstance selectedSeed;

    private void Start()
    {
        
        strainDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var strain in strainDatabase.strains)
            names.Add(strain.strainName);

        strainDropdown.AddOptions(names);
        RefreshSeedInventoryUI();
        RefreshSelectedTableUI();
    }

    private List<PlantStrainData> filteredStrains = new();

    private void OnSearchChanged(string text)
    {
        filteredStrains.Clear();

        foreach (var strain in strainDatabase.strains)
        {
            if (string.IsNullOrEmpty(text) || strain.strainName.ToLower().Contains(text.ToLower()))
                filteredStrains.Add(strain);
        }

        strainDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var strain in filteredStrains)
            names.Add(strain.strainName);

        strainDropdown.AddOptions(names);
    }

    // ==============================
    // DEBUG UI
    // ==============================

    public void AddMoneyButton()
    {
        return;
    }

    public void SpendMoneyButton()
    {
        return;
    }



    // ==============================
    // INVENTORY UI
    // ==============================

    public void RefreshSeedInventoryUI()
    {
        foreach (Transform child in seedPanelParent)
            Destroy(child.gameObject);

        List<SeedInstance> allSeeds = seedInventory.GetAllSeeds();

        foreach (SeedInstance seed in allSeeds)
        {
            GameObject btnObj = Instantiate(seedButtonPrefab, seedPanelParent);

            Button btn = btnObj.GetComponent<Button>();
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();

            btnText.text = seed.DisplayName;

            btn.onClick.AddListener(() => SelectSeed(seed));
        }
    }

    public void SelectSeed(SeedInstance seed)
    {
        selectedSeed = seed;
        selectedSeedText.text = seed.DisplayName;
    }

    // ==============================
    // TABLE UI
    // ==============================

    public void SelectTable(GrowTable table)
    {
        selectedTable = table;
        RefreshSelectedTableUI();
    }

    public void RefreshSelectedTableUI()
    {
        if (selectedTable == null)
        {
            selectedTableText.text = "No table selected";
            plantStatsText.text = "";
            return;
        }

        selectedTableText.text =
            $"Table Slots: {selectedTable.unlockedSlots}/{selectedTable.slots.Count}\n" +
            $"Lights: {selectedTable.lightQuality}\n" +
            $"Water: {selectedTable.waterQuality}";

        RefreshPlantStats();
    }

    public void RefreshPlantStats()
    {
        if (selectedTable == null)
            return;

        plantStatsText.text = "";

        for (int i = 0; i < selectedTable.slots.Count; i++)
        {
            TableSlot slot = selectedTable.slots[i];

            if (i >= selectedTable.unlockedSlots)
            {
                plantStatsText.text += $"Slot {i + 1}: LOCKED\n";
                continue;
            }

            if (slot.IsEmpty)
            {
                plantStatsText.text += $"Slot {i + 1}: Empty\n";
            }
            else
            {
                PlantInstance p = slot.currentPlant;

                string moldText = p.hasMold ? " | MOLD" : "";
                string pestText = p.hasPests ? " | PESTS" : "";

                string strainName = p.seed != null && p.seed.isMysterySeed ? "???" : p.strainData.strainName;

                plantStatsText.text +=
                    $"Slot {i + 1}: {strainName} ({p.seed.rarity})\n" +
                    $"Growth: {p.growthPercent:0}% | Ripeness: {p.ripenessPercent:0}%\n" +
                    $"Water: {p.waterLevel:0}% | Nutrients: {p.nutrientsLevel:0}%\n" +
                    $"Health: {p.health:0}% | Stress: {p.stress:0}%{moldText}{pestText}\n\n";
            }
        }
    }

    public void ShowSelectedStrainInfo()
    {
        int index = strainDropdown.value;
        PlantStrainData strain = strainDatabase.strains[index];

        plantStatsText.text =
            $"STRAIN: {strain.strainName}\n\n" +
            $"Water: {strain.idealWaterMin} - {strain.idealWaterMax}\n" +
            $"Nutrients: {strain.idealNutrientsMin} - {strain.idealNutrientsMax}\n" +
            $"Mold Susc: {strain.moldSusceptibility}\n" +
            $"Pest Susc: {strain.pestSusceptibility}\n\n" +
            $"Growth/Day: {strain.growthPerDay}\n" +
            $"Ripeness/Day: {strain.ripenessPerDayInFlower}\n\n" +
            $"Harvest Window: {strain.harvestWindowStart} - {strain.harvestWindowEnd}\n" +
            $"Overripe: {strain.overripeThreshold}\n\n" +
            $"Seed Cost: ${strain.seedCost}\n" +
            $"Pack5 Cost: ${strain.pack5Cost}\n" +
            $"Pack20 Cost: ${strain.pack20Cost}\n\n" +
            $"Payout Mult: {strain.payoutMultiplier}\n" +
            $"Genetics Score: {strain.geneticsScore}\n" +
            $"Shiny Chance: {strain.shinyChance}\n\n" +
            $"Description:\n{strain.description}";
    }

    // ==============================
    // PLANTING
    // ==============================

    public void PlantSelectedSeedButton()
    {
        if (selectedSeed == null)
        {
            selectedSeedText.text = "No seed selected.";
            return;
        }

        if (selectedTable == null)
        {
            selectedTableText.text = "No table selected.";
            return;
        }

        List<TableSlot> available = selectedTable.GetAvailableSlots();

        if (available.Count == 0)
        {
            selectedTableText.text = "No empty unlocked slots.";
            return;
        }

        bool removed = seedInventory.RemoveSpecificSeed(selectedSeed);

        if (!removed)
        {
            selectedSeedText.text = "Seed missing from inventory.";
            return;
        }

        TableSlot slot = available[0];

        PlantInstance plant = Instantiate(plantPrefab, slot.transform.position, Quaternion.identity);
        plant.transform.SetParent(slot.transform);

        plant.InitializeFromSeed(selectedSeed);

        slot.currentPlant = plant;

        selectedSeed = null;
        selectedSeedText.text = "Seed planted.";

        RefreshSeedInventoryUI();
        RefreshSelectedTableUI();
    }

    public void PlantAllSelectedSeedButton()
    {
        if (selectedSeed == null || selectedTable == null)
            return;

        PlantStrainData strain = selectedSeed.strain;

        List<TableSlot> available = selectedTable.GetAvailableSlots();
        if (available.Count == 0)
            return;

        int plantedCount = 0;

        foreach (TableSlot slot in available)
        {
            SeedInstance nextSeed = seedInventory.ConsumeSeed(strain);
            if (nextSeed == null)
                break;

            PlantInstance plant = Instantiate(plantPrefab, slot.transform.position, Quaternion.identity);
            plant.transform.SetParent(slot.transform);

            plant.InitializeFromSeed(nextSeed);

            slot.currentPlant = plant;
            plantedCount++;
        }

        selectedSeedText.text = $"Planted {plantedCount} seeds.";

        RefreshSeedInventoryUI();
        RefreshSelectedTableUI();
    }

    // ==============================
    // HARVEST
    // ==============================

    public void HarvestAllReadyButton()
    {
        if (selectedTable == null)
            return;

        int harvested = 0;

        foreach (TableSlot slot in selectedTable.slots)
        {
            if (slot.currentPlant == null)
                continue;

            PlantInstance plant = slot.currentPlant;

            if (!plant.IsHarvestable && !plant.IsOverripe)
                continue;

            harvested++;
            slot.RemovePlant();
        }

        plantStatsText.text = $"Harvested {harvested} plants.";

        RefreshSelectedTableUI();
    }

    // ==============================
    // TABLE UPGRADES
    // ==============================

    public void UpgradeTableSlots(int extraSlots)
    {
        if (selectedTable == null)
            return;

        selectedTable.UpgradeSlots(extraSlots);
        RefreshSelectedTableUI();
    }

    public void UpgradeTableLights(LightQuality newQuality)
    {
        if (selectedTable == null)
            return;

        selectedTable.lightQuality = newQuality;
        RefreshSelectedTableUI();
    }
}