// =================================================
// | WAREHOUSE HARVEST |
// |  @WASH3D x @MOBY  |
// =================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevSandboxControllerUI : MonoBehaviour
{
    // ==============================
    // REFERENCES (MANAGERS / DATABASES)
    // ==============================

    [Header("References")]
    public CollectionUIController collectionUIController;
    public DiscoveryManager discoveryManager;
    public EconomyManager economy;
    public PlantManager plantManager;
    public PlayerStatsManager playerStatsManager;
    public SaveLoadManager saveLoadManager;
    public SeedInventory seedInventory;
    public SeedShop seedShop;
    public TimeManager timeManager;
    public WarehouseManager warehouseManager;
    public WarehouseOverviewUIController warehouseOverviewUI;
    public StrainDatabase strainDatabase;

    [Header("Panels")]
    public GameObject collectionPanel;
    public GameObject debugPanel;
    public GameObject playerPanel;
    public GameObject realestatePanel;
    public GameObject shopPanel;
    public GameObject tradePanel;
    public GameObject warehousePanel;
    

    // ==============================
    // HUD UI (TOP BAR)
    // ==============================

    [Header("Game HUD")]
    public TMP_Text dayText;
    public TMP_Text moneyText;

    // ==============================
    // DROPDOWNS (WAREHOUSE / TABLE / STRAIN)
    // ==============================

    [Header("Warehouse Selection")]
    public TMP_Dropdown warehouseDropdown;
    public TMP_Dropdown tableDropdown;

    [Header("Top Bar Strain UI")]
    public TMP_InputField topSearchInput;
    public TMP_Dropdown topStrainDropdown;

    [Header("Shop Strain UI")]
    public TMP_InputField strainSearchInput;
    public TMP_Dropdown strainDropdown;

    // ==============================
    // TABLE SLOT LIST UI
    // ==============================

    [Header("Slot List UI")]
    public Transform slotListParent;
    public GameObject slotRowButtonPrefab;

    // ==============================
    // TABLE LIST UI (WAREHOUSE PANEL)
    // ==============================

    [Header("Table List UI")]
    public Transform tableListParent;
    public GameObject tableRowButtonPrefab;

    // ==============================
    // INVENTORY LIST UI
    // ==============================

    [Header("Seed Inventory UI")]
    public Transform inventoryListParent;
    public GameObject inventoryRowButtonPrefab;

    // ==============================
    // OUTPUT TEXT PANELS
    // ==============================

    [Header("Text Output")]
    public TMP_Text debugOutputText;
    public TMP_Text selectedInfoText;

    [Header("Table Info Panel")]
    public TMP_Text tablePanelText;

    [Header("Warehouse Detail Panel")]
    public TMP_Text warehousePanelText;

    [Header("Inventory Detail Panel")]
    public TMP_Text inventoryDetailText;

    // ==============================
    // PREFABS
    // ==============================

    [Header("Plant Prefab")]
    public PlantInstance plantPrefab;

    // ==============================
    // SELECTED STATE
    // ==============================

    private Warehouse selectedWarehouse;
    private GrowTable selectedTable;
    private TableSlot selectedSlot;

    private SeedInventorySummary selectedInventorySummary;

    // ==============================
    // INTERNAL CACHED LISTS
    // ==============================

    private List<Warehouse> foundWarehouses = new List<Warehouse>();
    private List<GrowTable> currentTables = new List<GrowTable>();
    private List<PlantStrainData> filteredStrains = new List<PlantStrainData>();

    // ==============================
    // UNITY START
    // ==============================

    private void Start()
    {
        if (topSearchInput != null)
            topSearchInput.onValueChanged.AddListener(OnTopSearchChanged);

        if (strainSearchInput != null)
            strainSearchInput.onValueChanged.AddListener(OnShopSearchChanged);

        BuildWarehouseDropdown();

        BuildTopStrainDropdown();
        BuildShopStrainDropdown();

        

        RefreshAllUI();

        Print("DevSandbox Controller Ready.");
    }

    // ==============================
    // MASTER UI REFRESH
    // ==============================

    // =============================
    // The single "source of truth" refresh call.
    // Anytime game state changes (buy, plant, upgrade, advance day),
    // call this instead of manually calling 6 refresh methods.
    // =============================
    private void RefreshAllUI()
    {
        RefreshHUD();
        RefreshWarehousePanel();
        RefreshTableList();
        RefreshTablePanel();
        RefreshSlotList();
        RefreshInventoryList();
        RefreshInventoryDetailPanel();
        UpdateSelectedInfoText();
        if (warehouseOverviewUI != null)
            warehouseOverviewUI.RefreshUI();
    }

    // ==============================
    // HUD
    // ==============================

    private void RefreshHUD()
    {
        if (economy != null && moneyText != null)
            moneyText.text = $"Money: ${economy.Money}";

        if (timeManager != null && dayText != null)
            dayText.text = $"Day: {timeManager.CurrentDay}";
    }

    private void OnTopSearchChanged(string text)
    {
        FilterStrains(text);
        RefreshTopDropdown();
    }

    private void OnShopSearchChanged(string text)
    {
        FilterStrains(text);
        RefreshShopDropdown();
    }

    private void FilterStrains(string searchText)
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
    }

    private void BuildTopStrainDropdown()
    {
        if (topStrainDropdown == null) return;

        filteredStrains.Clear();
        filteredStrains.AddRange(strainDatabase.strains);

        RefreshTopDropdown();
    }

    private void BuildShopStrainDropdown()
    {
        if (strainDropdown == null) return;

        filteredStrains.Clear();
        filteredStrains.AddRange(strainDatabase.strains);

        RefreshShopDropdown();
    }

    private void RefreshTopDropdown()
    {
        if (topStrainDropdown == null) return;

        topStrainDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var s in filteredStrains)
            names.Add(s.strainName);

        topStrainDropdown.AddOptions(names);
    }

    private void RefreshShopDropdown()
    {
        if (strainDropdown == null) return;

        strainDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var s in filteredStrains)
            names.Add(s.strainName);

        strainDropdown.AddOptions(names);
    }

    // ==============================
    // WAREHOUSE PANEL
    // ==============================

    private void RefreshWarehousePanel()
    {
        if (warehousePanelText == null)
            return;

        if (selectedWarehouse == null)
        {
            warehousePanelText.text = "No warehouse selected.";
            return;
        }

        int totalPlants = 0;
        int emptySlots = 0;
        int unlockedTables = 0;

        foreach (GrowTable table in selectedWarehouse.tables)
        {
            if (table == null)
                continue;

            if (table.isUnlocked)
                unlockedTables++;

            for (int i = 0; i < table.unlockedSlots; i++)
            {
                if (i >= table.slots.Count)
                    continue;

                if (table.slots[i].IsEmpty)
                    emptySlots++;
                else
                    totalPlants++;
            }
        }

        warehousePanelText.text =
            $"WAREHOUSE: {selectedWarehouse.name}\n" +
            $"Tables Unlocked: {unlockedTables}/{selectedWarehouse.tables.Count}\n" +
            $"Plants Growing: {totalPlants}\n" +
            $"Empty Slots: {emptySlots}";
    }

    // ==============================
    // TABLE PANEL
    // ==============================

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
            $"Unlocked: {(selectedTable.isUnlocked ? "YES" : "NO")}\n" +
            $"Slots: {selectedTable.unlockedSlots}/{selectedTable.slots.Count}\n" +
            $"Plants: {plantCount}\n" +
            $"Lights: {selectedTable.lightQuality}\n" +
            $"Water: {selectedTable.waterQuality}";
    }

    // ==============================
    // INVENTORY PANEL
    // ==============================

    public void RefreshInventoryList()
    {
        if (inventoryListParent == null || inventoryRowButtonPrefab == null)
            return;

        foreach (Transform child in inventoryListParent)
            Destroy(child.gameObject);

        if (seedInventory == null)
            return;

        List<SeedInventorySummary> summaries = seedInventory.GetSummaries();
        summaries.Sort((a, b) => b.totalCount.CompareTo(a.totalCount));

        foreach (var summary in summaries)
        {
            GameObject rowObj = Instantiate(inventoryRowButtonPrefab, inventoryListParent);

            Button btn = rowObj.GetComponent<Button>();
            TMP_Text txt = rowObj.GetComponentInChildren<TMP_Text>();

            if (btn == null || txt == null)
                continue;

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
    // WAREHOUSE + TABLE DROPDOWNS
    // ==============================

    private void BuildWarehouseDropdown()
    {
        foundWarehouses.Clear();
        foundWarehouses.AddRange(FindObjectsByType<Warehouse>());

        if (warehouseDropdown == null)
            return;

        warehouseDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var w in foundWarehouses)
            names.Add(w.name);

        warehouseDropdown.AddOptions(names);

        warehouseDropdown.onValueChanged.RemoveAllListeners();
        warehouseDropdown.onValueChanged.AddListener(OnWarehouseDropdownChanged);

        if (foundWarehouses.Count > 0)
        {
            warehouseDropdown.value = 0;
            OnWarehouseDropdownChanged(0);
        }
    }

    private void OnWarehouseDropdownChanged(int index)
    {
        if (index < 0 || index >= foundWarehouses.Count)
            return;

        selectedWarehouse = foundWarehouses[index];
        Print($"Selected Warehouse: {selectedWarehouse.name}");

        BuildTableDropdown();

        // IMPORTANT: Refresh everything after selection change
        RefreshAllUI();
    }

    private void BuildTableDropdown()
    {
        if (selectedWarehouse == null || tableDropdown == null)
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
        {
            tableDropdown.value = 0;
            OnTableDropdownChanged(0);
        }
    }

    private void OnTableDropdownChanged(int index)
    {
        if (index < 0 || index >= currentTables.Count)
            return;

        selectedTable = currentTables[index];
        selectedSlot = null;

        Print($"Selected Table: {selectedTable.name}");

        RefreshAllUI();
    }

    // ==============================
    // TABLE LIST PANEL (CLICKABLE TABLE ROWS)
    // ==============================

    public void RefreshTableList()
    {
        if (tableListParent == null || tableRowButtonPrefab == null)
            return;

        foreach (Transform child in tableListParent)
            Destroy(child.gameObject);

        if (selectedWarehouse == null)
            return;

        for (int i = 0; i < selectedWarehouse.tables.Count; i++)
        {
            GrowTable table = selectedWarehouse.tables[i];
            if (table == null)
                continue;

            GameObject rowObj = Instantiate(tableRowButtonPrefab, tableListParent);

            Button btn = rowObj.GetComponent<Button>();
            TMP_Text txt = rowObj.GetComponentInChildren<TMP_Text>();

            if (btn == null || txt == null)
                continue;

            int plantsGrowing = 0;
            for (int s = 0; s < table.unlockedSlots; s++)
            {
                if (s < table.slots.Count && !table.slots[s].IsEmpty)
                    plantsGrowing++;
            }

            bool isLocked = !table.isUnlocked;

            if (isLocked)
            {
                txt.text = $"{table.name}: LOCKED";
                btn.interactable = false;
            }
            else
            {
                txt.text = $"{table.name} ({table.unlockedSlots}/{table.slots.Count}) | Plants: {plantsGrowing}";
                btn.interactable = true;

                int capturedIndex = i;
                btn.onClick.AddListener(() =>
                {
                    selectedTable = selectedWarehouse.tables[capturedIndex];
                    selectedSlot = null;

                    Print($"Selected Table: {selectedTable.name}");
                    RefreshAllUI();
                });
            }
        }
    }

    // ==============================
    // STRAIN SEARCH
    // ==============================

    private void BuildStrainDropdown()
    {
        if (strainDatabase == null || strainDropdown == null)
            return;

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
        if (strainDatabase == null || strainDropdown == null)
            return;

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
        if (slotListParent == null || slotRowButtonPrefab == null)
            return;

        foreach (Transform child in slotListParent)
            Destroy(child.gameObject);

        if (selectedTable == null)
            return;

        for (int i = 0; i < selectedTable.slots.Count; i++)
        {
            TableSlot slot = selectedTable.slots[i];

            GameObject rowObj = Instantiate(slotRowButtonPrefab, slotListParent);

            Button btn = rowObj.GetComponent<Button>();
            TMP_Text txt = rowObj.GetComponentInChildren<TMP_Text>();

            if (btn == null || txt == null)
                continue;

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
            return;

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

    // ==============================
    // PLANTING (INVENTORY > SLOT)
    // ==============================

    private SeedInstance GetSeedFromSummary(SeedInventorySummary summary)
    {
        if (summary == null)
            return null;

        List<SeedInstance> allSeeds = seedInventory.GetAllSeeds();

        foreach (SeedInstance seed in allSeeds)
        {
            if (summary.isMystery && seed.isMysterySeed)
                return seed;

            if (!summary.isMystery && seed.strain == summary.strain)
                return seed;
        }

        return null;
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

        SeedInstance seedToPlant = GetSeedFromSummary(selectedInventorySummary);

        if (seedToPlant == null)
        {
            Print("No matching seed found in inventory.");
            return;
        }

        seedInventory.RemoveSpecificSeed(seedToPlant);

        PlantInstance plant = Instantiate(plantPrefab, selectedSlot.transform.position, Quaternion.identity);
        plant.transform.SetParent(selectedSlot.transform);
        plant.InitializeFromSeed(seedToPlant);

        selectedSlot.currentPlant = plant;

        Print($"Planted: {seedToPlant.DisplayName}");

        RefreshAllUI();
        
    }

    // ==============================
    // ECONOMY / SHOP
    // ==============================

    public void AddMoney(int amount)
    {
        economy.AddMoney(amount);
        Print($"Money added: +${amount}");
        RefreshAllUI();
    }

    public void SpendMoney(int amount)
    {
        economy.SpendMoney(amount);
        Print($"Money spent: -${amount}");
        RefreshAllUI();
    }

    public void BuySeedSingle()
    {
        PlantStrainData strain = GetSelectedStrain();
        if (strain == null)
            return;

        seedShop.BuySingleSeed(strain);
        Print($"Bought 1 seed: {strain.strainName}");

        RefreshAllUI();
    }

    public void BuyPack5()
    {
        PlantStrainData strain = GetSelectedStrain();
        if (strain == null)
            return;

        seedShop.BuyPack(strain, 5);
        Print($"Bought 5-pack: {strain.strainName}");

        RefreshAllUI();
    }

    public void BuyPack20()
    {
        PlantStrainData strain = GetSelectedStrain();
        if (strain == null)
            return;

        seedShop.BuyPack(strain, 20);
        Print($"Bought 20-pack: {strain.strainName}");

        RefreshAllUI();
    }

    public void BuyBagseedSingle()
    {
        seedShop.BuyBagseedPack(1);
        Print("Bought Bagseed Single.");
        RefreshInventoryList();
    }

    public void BuyBagseedPack5()
    {
        seedShop.BuyBagseedPack(5);
        Print("Bought Bagseed Pack (5).");

        RefreshAllUI();
    }

    public void BuyBagseedPack20()
    {
        seedShop.BuyBagseedPack(20);
        Print("Bought Bagseed Pack (20).");

        RefreshAllUI();
    }

    // ==============================
    // PLANT ACTIONS
    // ==============================

    public void WaterSelectedPlant()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        selectedSlot.currentPlant.WaterPlant(40f, 5f);
        Print("Watered selected plant.");

        RefreshAllUI();
    }

    public void FeedSelectedPlant()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        selectedSlot.currentPlant.FeedNutrients(35f);
        Print("Fed selected plant.");

        RefreshAllUI();
    }

    public void TreatSelectedMold()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        bool treated = selectedSlot.currentPlant.TreatMold();
        Print(treated ? "Mold treated." : "No mold present.");

        RefreshAllUI();
    }

    public void TreatSelectedPests()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        bool treated = selectedSlot.currentPlant.TreatPests();
        Print(treated ? "Pests treated." : "No pests present.");

        RefreshAllUI();
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

        // ============================
        // GRADING + DISCOVERY CHECK
        // ============================

        int score = HarvestGrader.CalculateScore(plant);
        string grade = HarvestGrader.GetGradeLetter(score);

        bool qualifiesForDiscovery = (score >= 700); // B or higher

        if (plant.seed != null && plant.seed.isMysterySeed)
        {
            if (qualifiesForDiscovery)
            {
                plant.seed.RevealMystery();
                discoveryManager.DiscoverStrain(plant.seed.strain);

                // NEW: Refresh the collection UI immediately
                if (collectionUIController != null) {
                    if (collectionUIController != null)
                    {
                        collectionUIController.RefreshList();
                        Print($"DISCOVERED: {plant.seed.strain.strainName} ({grade}) Score:{score}");
                    }
                    else
                    {
                        Debug.LogError("Collection UI Controller is NOT assigned in DevSandboxControllerUI!");
                    }
                }
                else
                {
                    Print("Mystery seed revealed but strain was null (bug).");
                }
            }
            else
            {
                Print($"Bagseed harvested ({grade}) Score:{score} - Not good enough to discover.");
            }
        }
        else
        {
            Print($"Harvested {plant.strainData.strainName} ({grade}) Score:{score}");
        }

        selectedSlot.RemovePlant();

        RefreshAllUI();
        collectionUIController.RefreshList();
    }

    public void PlantSelectedInventorySeedByRarity(int rarityInt)
    {
        if (selectedSlot == null || selectedTable == null)
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
            Print("No inventory strain selected.");
            return;
        }

        if (plantPrefab == null)
        {
            Print("Plant prefab missing.");
            return;
        }

        SeedRarity rarity = (SeedRarity)rarityInt;

        // Find matching seed in inventory
        SeedInstance chosenSeed = null;

        foreach (var seed in seedInventory.GetAllSeeds())
        {
            bool strainMatch =
                (selectedInventorySummary.isMystery && seed.isMysterySeed) ||
                (!selectedInventorySummary.isMystery && seed.strain == selectedInventorySummary.strain);

            if (!strainMatch)
                continue;

            if (seed.rarity == rarity)
            {
                chosenSeed = seed;
                break;
            }
        }

        if (chosenSeed == null)
        {
            Print($"No {rarity} seed available.");
            return;
        }

        // Remove from inventory
        seedInventory.RemoveSpecificSeed(chosenSeed);

        // Spawn plant
        PlantInstance plant = Instantiate(plantPrefab, selectedSlot.transform.position, Quaternion.identity);
        plant.transform.SetParent(selectedSlot.transform);

        plant.InitializeFromSeed(chosenSeed);
        selectedSlot.currentPlant = plant;

        Print($"Planted {chosenSeed.DisplayName}");

        RefreshSlotList();
        RefreshInventoryList();
        RefreshInventoryDetailPanel();
        RefreshTablePanel();
        RefreshWarehousePanel();
        UpdateSelectedInfoText();
    }

    // ==============================
    // TIME CONTROL
    // ==============================

    public void AdvanceDay()
    {
        timeManager.AdvanceDay();
        plantManager.AdvanceDayAll();

        Print("Day advanced.");
        RefreshAllUI();
    }

    // ==============================
    // UPGRADES / UNLOCKS
    // ==============================

    public void UnlockSelectedTable()
    {
        if (selectedTable == null)
        {
            Print("No table selected.");
            return;
        }

        bool success = selectedTable.UnlockTable(economy);
        Print(success ? "Table unlocked!" : "Failed to unlock table.");

        RefreshAllUI();
    }

    public void UpgradeSelectedTableSlots()
    {
        if (selectedTable == null)
            return;

        bool success = selectedTable.UpgradeSlots(economy, 1);
        Print(success ? "Slot upgraded!" : "Slot upgrade failed. All slots purchased.");

        RefreshAllUI();
    }

    public void UpgradeSelectedTableLights()
    {
        if (selectedTable == null)
            return;

        bool success = selectedTable.UpgradeLights(economy);
        Print(success ? "Lights upgraded!" : "Light upgrade failed. Max Upgrades.");

        RefreshAllUI();
    }

    public void UpgradeSelectedTableWater()
    {
        if (selectedTable == null)
            return;

        bool success = selectedTable.UpgradeWater(economy);
        Print(success ? "Water upgraded!" : "Water upgrade failed. Max Upgrades.");

        RefreshAllUI();
    }

    // ==============================
    // SAVE / LOAD
    // ==============================

    public void SaveGameButton()
    {
        saveLoadManager.SaveGame();
        Print("Game Saved.");
    }

    public void LoadGameButton()
    {
        saveLoadManager.LoadGame();
        Print("Game Loaded.");

        RefreshHUD();
        RefreshSlotList();
        RefreshTablePanel();
        RefreshWarehousePanel();
        RefreshInventoryList();
        RefreshInventoryDetailPanel();
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

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        BuildShopStrainDropdown();

        Print("Shop opened.");
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        Print("Shop closed.");
    }

    public void OpenCollection()
    {
        if (collectionPanel != null)
            collectionPanel.SetActive(true);
        collectionUIController.RefreshList();
    }

    public void OpenPlayer()
    {
        if (playerPanel != null)
        {
            playerPanel.SetActive(true);
        }
    }

    public void ClosePlayer()
    {
        if (playerPanel != null)
        {
            playerPanel.SetActive(false);
        }
    }

    public void Dev_DiscoverSelectedTopStrain()
    {
        if (discoveryManager == null)
        {
            Print("DiscoveryManager not assigned.");
            return;
        }

        if (topStrainDropdown == null)
        {
            Print("topStrainDropdown not assigned.");
            return;
        }

        int index = topStrainDropdown.value;

        if (filteredStrains == null || filteredStrains.Count == 0)
        {
            Print("No strains loaded in filteredStrains.");
            return;
        }

        if (index < 0 || index >= filteredStrains.Count)
        {
            Print("Invalid dropdown index.");
            return;
        }

        PlantStrainData strain = filteredStrains[index];

        if (strain == null)
        {
            Print("Selected strain is null.");
            return;
        }

        discoveryManager.DiscoverStrain(strain);

        Print($"DEV DISCOVERED: {strain.strainName}");

        RefreshAllUI();

        // If you have a Collection UI controller reference, refresh it too:
        if (collectionUIController != null)
            collectionUIController.RefreshList();
    }

    public void ForceSelectTable(GrowTable table)
    {
        if (table == null)
            return;

        selectedTable = table;
        selectedSlot = null;

        Print($"[WarehouseOverview] Selected Table: {selectedTable.name}");

        RefreshAllUI();
    }

}