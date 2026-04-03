using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevSandboxControllerUI : MonoBehaviour
{
    [Header("References")]
    public SeedInventory seedInventory;
    public EconomyManager economy;
    public SeedShop seedShop;
    public StrainDatabase strainDatabase;
    public PlantManager plantManager;
    public TimeManager timeManager;

    [Header ("Game HUD")]
    public TMP_Text dayText;
    public TMP_Text moneyText;
    
    [Header("Warehouse Selection")]
    public List<Warehouse> warehouses = new List<Warehouse>();
    public TMP_Dropdown warehouseDropdown;
    public TMP_Dropdown tableDropdown;

    [Header("Strain Search")]
    public TMP_InputField strainSearchInput;
    public TMP_Dropdown strainDropdown;

    [Header("Slot List UI")]
    public Transform slotListParent;
    public GameObject slotRowButtonPrefab;

    [Header("Text Output")]
    public TMP_Text debugOutputText;
    public TMP_Text selectedInfoText;

    [Header("Plant Prefab")]
    public PlantInstance plantPrefab;

    [Header("Table Info")]
    public TMP_Text tablePanelText;

    [Header("Seed Inventory UI")]
    public Transform inventoryListParent;
    public GameObject inventoryRowButtonPrefab;

    [Header("Inventory Detail Panel")]
    public TMP_Text inventoryDetailText;

    private SeedInventorySummary selectedInventorySummary;

    private SeedStack selectedSeedStack;

    private Warehouse selectedWarehouse;
    private GrowTable selectedTable;
    private TableSlot selectedSlot;


    private List<Warehouse> foundWarehouses = new List<Warehouse>();
    private List<GrowTable> currentTables = new List<GrowTable>();
    private List<PlantStrainData> filteredStrains = new List<PlantStrainData>();

    private void Start()
    {
        BuildWarehouseDropdown();
        BuildStrainDropdown();

        strainSearchInput.onValueChanged.AddListener(OnSearchChanged);
        RefreshHUD();
        Print("DevSandbox Controller Ready.");
    }

    // ==============================
    // REFRESH DEVSANDBOX UI / HUD
    // ==============================

    private void RefreshHUD()
    {
        if (moneyText != null)
            moneyText.text = $"Money: ${economy.Money}";

        if (dayText != null)
            dayText.text = $"Day: {timeManager.CurrentDay}";
    }

    private void RefreshTablePanel()
    {
        if (tablePanelText == null)
            return;

        if (selectedTable == null)
        {
            tablePanelText.text = "No table selected.";
            return;
        }

        int plantCount = 0;
        foreach (var slot in selectedTable.slots)
            if (!slot.IsEmpty) plantCount++;

        tablePanelText.text =
            $"TABLE: {selectedTable.name}\n" +
            $"Slots: {selectedTable.unlockedSlots}/{selectedTable.slots.Count}\n" +
            $"Plants: {plantCount}\n" +
            $"Lights: {selectedTable.lightQuality}\n" +
            $"Water: {selectedTable.waterQuality}";
    }

    public void RefreshInventoryList()
    {
        if (inventoryListParent == null || inventoryRowButtonPrefab == null)
            return;

        foreach (Transform child in inventoryListParent)
            Destroy(child.gameObject);

        List<SeedInventorySummary> summaries = seedInventory.GetSummaries();

        // Sort biggest stacks first
        summaries.Sort((a, b) => b.totalCount.CompareTo(a.totalCount));

        foreach (var summary in summaries)
        {
            GameObject rowObj = Instantiate(inventoryRowButtonPrefab, inventoryListParent);

            Button btn = rowObj.GetComponent<Button>();
            TMP_Text txt = rowObj.GetComponentInChildren<TMP_Text>();

            txt.text = summary.DisplayName;

            btn.onClick.AddListener(() =>
            {
                selectedInventorySummary = summary;
                RefreshInventoryDetailPanel();
            });
        }
    }

    public void RefreshInventoryDetailPanel()
    {
        if (inventoryDetailText == null)
            return;

        if (selectedInventorySummary == null)
        {
            inventoryDetailText.text = "No seed selected.";
            return;
        }

        var s = selectedInventorySummary;

        string name = s.isMystery ? "??? Bagseed" : s.strain.strainName;

        inventoryDetailText.text =
            $"SEED INVENTORY\n" +
            $"-----------------\n" +
            $"Strain: {name}\n" +
            $"Total: {s.totalCount}\n\n" +
            $"Common: {s.rarityCounts[SeedRarity.Common]}\n" +
            $"Uncommon: {s.rarityCounts[SeedRarity.Uncommon]}\n" +
            $"Rare: {s.rarityCounts[SeedRarity.Rare]}\n" +
            $"Epic: {s.rarityCounts[SeedRarity.Epic]}\n" +
            $"Legendary: {s.rarityCounts[SeedRarity.Legendary]}\n\n" +
            $"Shiny Total: {s.shinyCount}";
    }

    // ==============================
    // WAREHOUSE / TABLE SELECTION
    // ==============================

    private void BuildWarehouseDropdown()
    {
        foundWarehouses.Clear();
        foundWarehouses.AddRange(FindObjectsByType<Warehouse>());

        warehouseDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var w in foundWarehouses)
            names.Add(w.name);

        warehouseDropdown.AddOptions(names);
        warehouseDropdown.onValueChanged.RemoveAllListeners();
        warehouseDropdown.onValueChanged.AddListener(OnWarehouseDropdownChanged);

        if (foundWarehouses.Count > 0)
            warehouseDropdown.value = 0;

        OnWarehouseDropdownChanged(warehouseDropdown.value);
    }


    private void OnWarehouseDropdownChanged(int index)
    {
        if (index < 0 || index >= foundWarehouses.Count)
            return;

        selectedWarehouse = foundWarehouses[index];
        Print($"Selected Warehouse: {selectedWarehouse.name}");

        BuildTableDropdown();
        RefreshTablePanel();
    }
  
    private void SetWarehouse(int index)
    {
        if (index < 0 || index >= warehouses.Count)
            return;

        selectedWarehouse = warehouses[index];
        Print($"Selected Warehouse: {selectedWarehouse.name}");

        BuildTableDropdown();
    }

    private void BuildTableDropdown()
    {
        if (selectedWarehouse == null)
            return;

        currentTables.Clear();
        currentTables.AddRange(selectedWarehouse.tables);

        tableDropdown.ClearOptions();

        List<string> tableNames = new List<string>();
        for (int i = 0; i < currentTables.Count; i++)
        {
            GrowTable t = currentTables[i];
            tableNames.Add($"{t.name} ({t.unlockedSlots}/{t.slots.Count})");
        }

        tableDropdown.AddOptions(tableNames);
        tableDropdown.onValueChanged.RemoveAllListeners();
        tableDropdown.onValueChanged.AddListener(OnTableDropdownChanged);

        if (currentTables.Count > 0)
            tableDropdown.value = 0;

        OnTableDropdownChanged(tableDropdown.value);
    }

    private void OnTableDropdownChanged(int index)
    {
        if (index < 0 || index >= currentTables.Count)
            return;

        selectedTable = currentTables[index];
        selectedSlot = null;

        Print($"Selected Table: {selectedTable.name}");

        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshTablePanel();
    }

    private void OnTableChanged(int index)
    {
        SetTable(index);
    }

    private void SetTable(int index)
    {
        if (selectedWarehouse == null)
            return;

        if (index < 0 || index >= selectedWarehouse.tables.Count)
            return;

        selectedTable = selectedWarehouse.tables[index];
        selectedSlot = null;

        Print($"Selected Table: {selectedTable.name}");
        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshTablePanel();
    }

    // ==============================
    // STRAIN SEARCH
    // ==============================

    private void BuildStrainDropdown()
    {
        filteredStrains.Clear();
        filteredStrains.AddRange(strainDatabase.strains);

        strainDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var strain in filteredStrains)
            names.Add(strain.strainName);

        strainDropdown.AddOptions(names);
    }

    private void OnSearchChanged(string searchText)
    {
        filteredStrains.Clear();

        foreach (var strain in strainDatabase.strains)
        {
            if (string.IsNullOrEmpty(searchText) ||
                strain.strainName.ToLower().Contains(searchText.ToLower()))
            {
                filteredStrains.Add(strain);
            }
        }

        strainDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var strain in filteredStrains)
            names.Add(strain.strainName);

        strainDropdown.AddOptions(names);

        Print($"Search results: {filteredStrains.Count} strains.");
    }

    private PlantStrainData GetSelectedStrain()
    {
        if (filteredStrains == null || filteredStrains.Count == 0)
            return null;

        int index = strainDropdown.value;
        if (index < 0 || index >= filteredStrains.Count)
            return null;

        return filteredStrains[index];
    }

    // ==============================
    // SLOT LIST UI
    // ==============================

    public void RefreshSlotList()
    {
        if (slotListParent == null)
        {
            Debug.LogError("slotListParent is not assigned in DevSandboxControllerUI!");
            return;
        }

        if (slotRowButtonPrefab == null)
        {
            Debug.LogError("slotRowButtonPrefab is not assigned in DevSandboxControllerUI!");
            return;
        }

        foreach (Transform child in slotListParent)
            Destroy(child.gameObject);

        if (selectedTable == null)
            return;

        for (int i = 0; i < selectedTable.slots.Count; i++)
        {
            TableSlot slot = selectedTable.slots[i];

            GameObject rowObj = Instantiate(slotRowButtonPrefab, slotListParent);

            Button btn = rowObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("SlotRowButtonPrefab is missing a Button component!");
                return;
            }

            TMP_Text txt = rowObj.GetComponentInChildren<TMP_Text>();
            if (txt == null)
            {
                Debug.LogError("SlotRowButtonPrefab is missing TMP_Text component!");
                return;
            }

            string label;

            if (i >= selectedTable.unlockedSlots)
            {
                label = $"Slot {i + 1}: LOCKED";
            }
            else if (slot.IsEmpty)
            {
                label = $"Slot {i + 1}: Empty";
            }
            else
            {
                PlantInstance plant = slot.currentPlant;

                string strainName = "Unknown";

                if (plant.seed != null && plant.seed.isMysterySeed)
                    strainName = "???";
                else if (plant.strainData != null)
                    strainName = plant.strainData.strainName;

                label = $"Slot {i + 1}: {strainName} | Growth:{plant.growthPercent:0}% | Health:{plant.health:0}% | Stress:{plant.stress:0}%";
            }

            txt.text = label;

            int capturedIndex = i;
            btn.onClick.AddListener(() => SelectSlot(capturedIndex));
        }

        RefreshTablePanel();
    }
    private void SelectSlot(int index)
    {
        if (selectedTable == null)
            return;

        if (index < 0 || index >= selectedTable.slots.Count)
            return;

        selectedSlot = selectedTable.slots[index];
        Print($"Selected Slot {index + 1}");

        UpdateSelectedInfoText();
    }

    private void UpdateSelectedInfoText()
    {
        if (selectedInfoText == null)
        {
            Debug.LogError("selectedInfoText is not assigned in DevSandboxControllerUI!");
            return;
        }

        if (selectedSlot == null)
        {
            selectedInfoText.text = "No slot selected.";
            return;
        }

        if (selectedTable == null)
        {
            selectedInfoText.text = "No table selected.";
            return;
        }

        int slotIndex = selectedTable.slots.IndexOf(selectedSlot);

        if (slotIndex == -1)
        {
            selectedInfoText.text = "Selected slot not found in table.";
            return;
        }

        if (slotIndex >= selectedTable.unlockedSlots)
        {
            selectedInfoText.text = $"Slot {slotIndex + 1} is LOCKED.";
            return;
        }

        if (selectedSlot.IsEmpty)
        {
            selectedInfoText.text = $"Slot {slotIndex + 1} is Empty.";
            return;
        }

        PlantInstance plant = selectedSlot.currentPlant;

        string strainName = "Unknown";
        string rarity = "Unknown";
        string shiny = "";

        if (plant.seed != null)
        {
            rarity = plant.seed.rarity.ToString();
            shiny = plant.seed.isShiny ? "Shiny" : "";

            if (plant.seed.isMysterySeed)
                strainName = "???";
            else if (plant.strainData != null)
                strainName = plant.strainData.strainName;
        }
        else
        {
            if (plant.strainData != null)
                strainName = plant.strainData.strainName;
        }

        selectedInfoText.text =
            $"SLOT {slotIndex + 1}\n" +
            $"Strain: {strainName} {shiny}\n" +
            $"Rarity: {rarity}\n\n" +
            $"Growth: {plant.growthPercent:0}%\n" +
            $"Ripeness: {plant.ripenessPercent:0}%\n\n" +
            $"Water: {plant.waterLevel:0}%\n" +
            $"Nutrients: {plant.nutrientsLevel:0}%\n\n" +
            $"Health: {plant.health:0}%\n" +
            $"Stress: {plant.stress:0}%\n\n" +
            $"Mold: {(plant.hasMold ? "YES" : "No")}\n" +
            $"Pests: {(plant.hasPests ? "YES" : "No")}";
    }

    public void PlantSelectedStrainToSelectedSlot()
    {
        if (selectedTable == null || selectedSlot == null)
        {
            Print("No table/slot selected.");
            return;
        }

        if (!selectedSlot.IsEmpty)
        {
            Print("Slot already occupied.");
            return;
        }

        PlantStrainData strain = GetSelectedStrainFromDropdown();
        if (strain == null)
        {
            Print("No strain selected.");
            return;
        }

        // Create seed instance
        SeedInstance seed = new SeedInstance();
        seed.strain = strain;
        seed.rarity = SeedRarity.Common;
        seed.isMysterySeed = false;
        seed.isShiny = Random.value < strain.shinyChance;

        // Spawn plant prefab into slot
        PlantInstance plant = Instantiate(plantPrefab, selectedSlot.transform.position, Quaternion.identity);
        plant.transform.SetParent(selectedSlot.transform);

        plant.InitializeFromSeed(seed);

        selectedSlot.currentPlant = plant;

        Print($"Planted {strain.strainName} into slot.");

        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshTablePanel();
        RefreshInventoryList();
    }

    private PlantStrainData GetSelectedStrainFromDropdown()
    {
        if (strainDropdown == null || strainDatabase == null)
            return null;

        int index = strainDropdown.value;

        if (index < 0 || index >= filteredStrains.Count)
            return null;

        return filteredStrains[index];
    }

    //public void PlantRandomSeedFromInventory()
    //{
    //    if (selectedSlot == null || selectedSlot.currentPlant != null)
    //        return;

    //    List<SeedInstance> seeds = seedInventory.GetAllSeeds();
    //    if (seeds.Count == 0)
    //    {
    //        Print("No seeds in inventory.");
    //        return;
    //    }

    //    SeedInstance seed = seeds[0]; // first one
    //    seedInventory.RemoveSpecificSeed(seed);

    //    PlantInstance plant = Instantiate(plantPrefab, selectedSlot.transform.position, Quaternion.identity);
    //    plant.transform.SetParent(selectedSlot.transform);

    //    plant.InitializeFromSeed(seed);
    //    selectedSlot.currentPlant = plant;

    //    Print($"Planted seed: {seed.DisplayName}");

    //    RefreshSlotList();
    //    UpdateSelectedInfoText();
    //    RefreshTablePanel();
    //    RefreshInventoryList();
    //}

    // ==============================
    // ECONOMY / SHOP
    // ==============================

    public void AddMoney(int amount)
    {
        economy.AddMoney(amount);
        Print($"Money added: +${amount}. Balance: ${economy.Money}");
        RefreshSlotList();
        RefreshHUD();
    }

    public void SpendMoney(int amount)
    {
        economy.SpendMoney(amount);
        Print($"Money subtracted: -${amount}. Balance: ${economy.Money}");
        RefreshSlotList();
        RefreshHUD();
    }

    public void BuySeedSingle()
    {
        PlantStrainData strain = GetSelectedStrain();
        if (strain == null) return;

        seedShop.BuySingleSeed(strain);
        Print($"Bought 1 seed: {strain.strainName}");

        RefreshSlotList();
        RefreshInventoryList();
    }

    public void BuyPack5()
    {
        PlantStrainData strain = GetSelectedStrain();
        if (strain == null) return;

        seedShop.BuyPack(strain, 5);
        Print($"Bought 5-pack: {strain.strainName}");
        RefreshInventoryList();
    }

    public void BuyPack20()
    {
        PlantStrainData strain = GetSelectedStrain();
        if (strain == null) return;

        seedShop.BuyPack(strain, 20);
        Print($"Bought 20-pack: {strain.strainName}");
        RefreshInventoryList();
    }

    public void BuyBagseedPack5()
    {
        seedShop.BuyBagseedPack(5);
        Print("Bought Bagseed Pack (5).");
        RefreshInventoryList();
    }

    public void BuyBagseedPack20()
    {
        seedShop.BuyBagseedPack(20);
        Print("Bought Bagseed Pack (20).");
        RefreshInventoryList();
    }

    private SeedInstance GetSeedFromSummary(SeedInventorySummary summary)
    {
        if (summary == null)
            return null;

        List<SeedInstance> allSeeds = seedInventory.GetAllSeeds();

        foreach (SeedInstance seed in allSeeds)
        {
            // Mystery bagseed selection
            if (summary.isMystery && seed.isMysterySeed)
                return seed;

            // Normal strain selection
            if (!summary.isMystery && seed.strain == summary.strain)
                return seed;
        }

        return null;
    }

    // ==============================
    // PLANT ACTIONS
    // ==============================

    public void PlantSelectedStrainIntoEmptySlot()
    {
        if (selectedTable == null)
            return;

        PlantStrainData strain = GetSelectedStrain();
        if (strain == null)
            return;

        List<TableSlot> available = selectedTable.GetAvailableSlots();
        if (available.Count == 0)
        {
            Print("No empty slots.");
            return;
        }

        SeedInstance seed = seedInventory.ConsumeSeed(strain);
        if (seed == null)
        {
            Print("No seed in inventory for that strain.");
            return;
        }

        TableSlot slot = available[0];

        PlantInstance plant = Instantiate(plantPrefab, slot.transform.position, Quaternion.identity);
        plant.transform.SetParent(slot.transform);

        plant.InitializeFromSeed(seed);
        slot.currentPlant = plant;

        Print($"Planted {seed.DisplayName}");
        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshTablePanel();
        RefreshInventoryList();
    }

    public void PlantAllEmptySlotsSelectedStrain()
    {
        if (selectedTable == null)
            return;

        PlantStrainData strain = GetSelectedStrain();
        if (strain == null)
            return;

        List<TableSlot> available = selectedTable.GetAvailableSlots();
        if (available.Count == 0)
            return;

        int planted = 0;

        foreach (var slot in available)
        {
            SeedInstance seed = seedInventory.ConsumeSeed(strain);
            if (seed == null)
                break;

            PlantInstance plant = Instantiate(plantPrefab, slot.transform.position, Quaternion.identity);
            plant.transform.SetParent(slot.transform);

            plant.InitializeFromSeed(seed);
            slot.currentPlant = plant;

            planted++;
        }

        Print($"Planted {planted} seeds of {strain.strainName}");
        RefreshSlotList();
        RefreshTablePanel();
        RefreshInventoryList();
    }

    public void PlantSelectedInventorySeedToSelectedSlot()
    {
        if (selectedTable == null || selectedSlot == null)
        {
            Print("No table/slot selected.");
            return;
        }

        int slotIndex = selectedTable.slots.IndexOf(selectedSlot);
        if (slotIndex >= selectedTable.unlockedSlots)
        {
            Print("Slot is locked.");
            return;
        }

        if (!selectedSlot.IsEmpty)
        {
            Print("Slot already occupied.");
            return;
        }

        if (selectedInventorySummary == null)
        {
            Print("No inventory seed type selected.");
            return;
        }

        // Get ONE real seed from inventory
        SeedInstance seedToPlant = GetSeedFromSummary(selectedInventorySummary);

        if (seedToPlant == null)
        {
            Print("No matching seed found in inventory.");
            return;
        }

        // Remove from inventory
        seedInventory.RemoveSpecificSeed(seedToPlant);

        // Spawn plant
        PlantInstance plant = Instantiate(plantPrefab, selectedSlot.transform.position, Quaternion.identity);
        plant.transform.SetParent(selectedSlot.transform);

        // Initialize plant
        plant.InitializeFromSeed(seedToPlant);

        // Assign plant to slot
        selectedSlot.currentPlant = plant;

        Print($"Planted: {seedToPlant.DisplayName}");

        // Refresh UI
        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshTablePanel();
        RefreshInventoryList();
        RefreshInventoryDetailPanel();
    }

    public void WaterSelectedPlant()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        selectedSlot.currentPlant.WaterPlant(40f, 5f);

        Print("Watered selected plant.");
        RefreshSlotList();
        UpdateSelectedInfoText();
    }

    public void FeedSelectedPlant()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        selectedSlot.currentPlant.FeedNutrients(35f);

        Print("Fed selected plant.");
        RefreshSlotList();
        UpdateSelectedInfoText();
    }

    public void TreatSelectedMold()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        bool treated = selectedSlot.currentPlant.TreatMold();
        Print(treated ? "Mold treated." : "No mold present.");
        RefreshSlotList();
        UpdateSelectedInfoText();
    }

    public void TreatSelectedPests()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        bool treated = selectedSlot.currentPlant.TreatPests();
        Print(treated ? "Pests treated." : "No pests present.");
        RefreshSlotList();
        UpdateSelectedInfoText();
    }

    public void HarvestSelectedPlant()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        PlantInstance plant = selectedSlot.currentPlant;

        if (!plant.IsHarvestable && !plant.IsOverripe)
        {
            Print("Plant is not harvestable.");
            return;
        }

        selectedSlot.RemovePlant();
        Print("Harvested plant.");

        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshInventoryList();
    }

    // ==============================
    // TIME CONTROL
    // ==============================

    public void AdvanceDay()
    {
        timeManager.AdvanceDay();
        plantManager.AdvanceDayAll();
        Print("Day advanced. > DevSandboxController > AdvanceDay");

        RefreshHUD();
        RefreshSlotList();
        UpdateSelectedInfoText();
        RefreshTablePanel();
    }

    // ==============================
    // DEBUG OUTPUT
    // ==============================

    private void Print(string msg)
    {
        Debug.Log(msg);

        if (debugOutputText != null)
            debugOutputText.text = msg;
    }
}