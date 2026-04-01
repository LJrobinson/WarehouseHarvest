using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DevSandboxUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private PlantManager plantManager;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private StrainUnlockManager unlockManager;
    [SerializeField] private PackLoyaltyManager loyaltyManager;
    [SerializeField] private GrowEquipmentManager equipmentManager;

    [Header("Database")]
    [SerializeField] private StrainDatabase strainDatabase;

    [Header("UI Text")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text plantText;
    [SerializeField] private TMP_Text harvestText;

    [Header("Shop UI")]
    [SerializeField] private TMP_Dropdown strainDropdown;
    [SerializeField] private TMP_Text strainInfoText;
    [SerializeField] private TMP_Text seedCountText;
    [SerializeField] private SeedInventoryUI seedInventoryUI;

    private PlantStrainData selectedStrain;
    private List<PlantStrainData> dropdownStrains = new List<PlantStrainData>();

    private void Start()
    {
        SetupDropdown();
        RefreshUI();
    }

    public void AddMoneyButton()
    {
        economyManager.AddMoney(1000000);
        RefreshUI();
    }

    public void SpendMoneyButton()
    {
        economyManager.SpendMoney(25);
        RefreshUI();
    }

    private void SetupDropdown()
    {
        if (strainDatabase == null || strainDatabase.strains.Count == 0)
        {
            Debug.LogError("No strainDatabase assigned or no strains in database!");
            return;
        }

        if (unlockManager == null)
        {
            Debug.LogError("No unlockManager assigned!");
            return;
        }

        dropdownStrains.Clear();
        strainDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (PlantStrainData strain in strainDatabase.strains)
        {
            if (unlockManager.IsUnlocked(strain))
            {
                dropdownStrains.Add(strain);
                options.Add(strain.strainName);
            }
        }

        strainDropdown.onValueChanged.RemoveAllListeners();

        if (dropdownStrains.Count == 0)
        {
            strainInfoText.text = "No strains unlocked yet.\nBuy bagseed to discover strains!";
            seedCountText.text = "";
            selectedStrain = null;
            return;
        }

        strainDropdown.AddOptions(options);
        strainDropdown.onValueChanged.AddListener(OnDropdownChanged);

        selectedStrain = dropdownStrains[0];
        UpdateSelectedStrainUI();
    }

    private void OnDropdownChanged(int index)
    {
        if (dropdownStrains.Count == 0)
            return;

        selectedStrain = dropdownStrains[index];
        UpdateSelectedStrainUI();
    }

    private void UpdateSelectedStrainUI()
    {
        if (selectedStrain == null)
        {
            strainInfoText.text = "No strain selected.";
            seedCountText.text = "";
            return;
        }

        strainInfoText.text =
            $"{selectedStrain.strainName}\n" +
            $"Seed Cost: ${selectedStrain.seedCost}\n" +
            $"5-Pack: ${selectedStrain.pack5Cost}\n" +
            $"20-Pack: ${selectedStrain.pack20Cost}\n" +
            $"Growth/Day: {selectedStrain.growthPerDay}\n" +
            $"Ripeness/Day: {selectedStrain.ripenessPerDayInFlower}\n" +
            $"Harvest Window: {selectedStrain.harvestWindowStart}-{selectedStrain.harvestWindowEnd}\n" +
            $"Shiny Chance: {(selectedStrain.shinyChance * 100f):0.00}%\n" +
            $"Payout Mult: x{selectedStrain.payoutMultiplier:0.00}";

        int ownedSeeds = seedInventory.GetSeedCount(selectedStrain);
        seedCountText.text = $"Seeds Owned: {ownedSeeds}";
    }

    private PlantStrainData GetRandomStrainFromDatabase()
    {
        if (strainDatabase == null || strainDatabase.strains.Count == 0)
            return null;

        return strainDatabase.strains[Random.Range(0, strainDatabase.strains.Count)];
    }

    // ==========================
    // SHOP: DIRECT PURCHASE
    // ==========================

    public void BuySeedButton()
    {
        if (selectedStrain == null)
            return;

        bool success = economyManager.SpendMoney(selectedStrain.seedCost);

        if (!success)
        {
            harvestText.text = "Not enough money to buy seed.";
            return;
        }

        SeedInstance newSeed = SeedGenerator.GenerateSeed(selectedStrain);
        seedInventory.AddSeed(newSeed);

        harvestText.text = $"Bought seed: {newSeed.DisplayName}";

        RefreshUI();
        RefreshSeedListUI();
    }

    public void Buy5PackButton()
    {
        if (selectedStrain == null)
            return;

        BuySeedPack(selectedStrain, 5, selectedStrain.pack5Cost, 0.03f, false);
    }

    public void Buy20PackButton()
    {
        if (selectedStrain == null)
            return;

        bool success = BuySeedPack(selectedStrain, 20, selectedStrain.pack20Cost, 0.06f, true);

        if (success && loyaltyManager != null)
        {
            loyaltyManager.RegisterTwentyPackPurchase();

            if (loyaltyManager.HasEarnedBonus())
            {
                loyaltyManager.ResetStreak();
                GrantBonusPack();
            }
        }
    }

    private bool BuySeedPack(PlantStrainData strain, int amount, int packCost, float rarityBoost, bool guaranteedRare)
    {
        bool success = economyManager.SpendMoney(packCost);

        if (!success)
        {
            harvestText.text = "Not enough money to buy pack.";
            return false;
        }

        int shinyCount = 0;
        int rarePlusCount = 0;

        for (int i = 0; i < amount; i++)
        {
            SeedInstance seed;

            if (guaranteedRare && i == amount - 1)
            {
                seed = SeedGenerator.GenerateSeedWithMinimumRarity(strain, SeedRarity.Rare, rarityBoost);
            }
            else
            {
                seed = SeedGenerator.GenerateSeed(strain, rarityBoost);
            }

            seedInventory.AddSeed(seed);

            if (seed.isShiny)
                shinyCount++;

            if (seed.rarity >= SeedRarity.Rare)
                rarePlusCount++;
        }

        harvestText.text = $"Bought {amount}-Pack: {rarePlusCount} Rare+ / {shinyCount} Shiny";

        RefreshUI();
        RefreshSeedListUI();
        return true;
    }

    private void GrantBonusPack()
    {
        PlantStrainData randomStrain = GetRandomStrainFromDatabase();

        if (randomStrain == null)
            return;

        int shinyCount = 0;
        int rarePlusCount = 0;

        for (int i = 0; i < 5; i++)
        {
            SeedInstance seed = SeedGenerator.GenerateSeed(randomStrain, 0.05f);
            seedInventory.AddSeed(seed);

            if (seed.isShiny)
                shinyCount++;

            if (seed.rarity >= SeedRarity.Rare)
                rarePlusCount++;
        }

        harvestText.text = $"BONUS PACK! Free Mystery 5-Pack: {randomStrain.strainName} ({rarePlusCount} Rare+ / {shinyCount} Shiny)";
    }

    // ==========================
    // SHOP: BAGSEED / MYSTERY
    // ==========================

    public void BuyBagseed5Pack()
    {
        BuyBagseedPack(5, 75);
    }

    public void BuyBagseed20Pack()
    {
        BuyBagseedPack(20, 250);
    }

    private void BuyBagseedPack(int amount, int cost)
    {
        bool success = economyManager.SpendMoney(cost);

        if (!success)
        {
            harvestText.text = "Not enough money for bagseed pack.";
            return;
        }

        int shinyCount = 0;
        int rarePlusCount = 0;

        for (int i = 0; i < amount; i++)
        {
            PlantStrainData randomStrain = GetRandomStrainFromDatabase();
            if (randomStrain == null)
                continue;

            SeedInstance seed = SeedGenerator.GenerateSeed(randomStrain, 0.02f);
            seed.isMysterySeed = true;

            seedInventory.AddSeed(seed);

            if (seed.isShiny)
                shinyCount++;

            if (seed.rarity >= SeedRarity.Rare)
                rarePlusCount++;
        }

        harvestText.text = $"Bought Bagseed {amount}-Pack: {rarePlusCount} Rare+ / {shinyCount} Shiny";

        RefreshUI();
        RefreshSeedListUI();
    }

    // ==========================
    // PLANTING / GAMEPLAY
    // ==========================

    public void PlantSeedButton()
    {
        if (selectedStrain == null)
            return;

        if (plantManager.HasPlant)
        {
            harvestText.text = "A plant is already growing.";
            return;
        }

        SeedInstance seed = seedInventory.ConsumeSeed(selectedStrain);

        if (seed == null)
        {
            harvestText.text = "No seeds owned for that strain.";
            return;
        }

        plantManager.SpawnPlantFromSeed(seed);

        harvestText.text = $"Planted: {seed.DisplayName}";

        RefreshSeedListUI();
    }

    public void WaterPlantButton()
    {
        PlantInstance plant = plantManager.CurrentPlant;
        if (plant == null) return;

        float stressReduction = equipmentManager != null ? equipmentManager.GetWaterStressReduction() : 0f;

        plant.WaterPlant(40f, stressReduction);
        harvestText.text = "Watered plant.";
        RefreshUI();
    }

    public void FeedNutrientsButton()
    {
        PlantInstance plant = plantManager.CurrentPlant;
        if (plant == null) return;

        plant.FeedNutrients(35f);
        harvestText.text = "Fed nutrients.";
        RefreshUI();
    }

    public void TreatMoldButton()
    {
        PlantInstance plant = plantManager.CurrentPlant;
        if (plant == null) return;

        if (plant.TreatMold())
            harvestText.text = "Applied fungicide (mold treated).";
        else
            harvestText.text = "No mold to treat.";

        RefreshUI();
    }

    public void TreatPestsButton()
    {
        PlantInstance plant = plantManager.CurrentPlant;
        if (plant == null) return;

        if (plant.TreatPests())
            harvestText.text = "Applied pesticide (pests treated).";
        else
            harvestText.text = "No pests to treat.";

        RefreshUI();
    }

    public void AdvanceDayButton()
    {
        timeManager.AdvanceDay();
        plantManager.AdvanceDayForPlant();
        RefreshUI();
    }

    public void HarvestButton()
    {
        PlantInstance plant = plantManager.CurrentPlant;

        if (plant == null)
        {
            harvestText.text = "No plant to harvest.";
            return;
        }

        if (!plant.IsHarvestable && !plant.IsOverripe)
        {
            harvestText.text = $"Not ready! Stage: {plant.stage}";
            return;
        }

        int score = HarvestGrader.CalculateScore(plant);
        string grade = HarvestGrader.GetGradeLetter(score);

        float rarityMult = SeedGenerator.GetPayoutMultiplierBonus(plant.seed.rarity, plant.seed.isShiny);
        float multiplier = plant.strainData.payoutMultiplier * rarityMult;

        int payout = Mathf.RoundToInt(score * 0.5f * multiplier);

        economyManager.AddMoney(payout);

        PlantStrainData revealedStrain = plant.strainData;
        bool wasMystery = plant.seed.isMysterySeed;

        // Reveal strain
        plant.seed.isMysterySeed = false;

        bool newlyDiscovered = false;
        bool newlyUnlocked = false;

        if (unlockManager != null)
        {
            newlyDiscovered = unlockManager.DiscoverStrain(revealedStrain);

            // Unlock threshold (B or better)
            if (score >= 700)
                newlyUnlocked = unlockManager.UnlockStrain(revealedStrain);
        }

        if (wasMystery)
        {
            if (newlyDiscovered)
            {
                harvestText.text =
                    $" NEW STRAIN DISCOVERED: {revealedStrain.strainName}!\n" +
                    $"Grade {grade} ({score}/1000) +${payout}";
            }
            else
            {
                harvestText.text =
                    $"Revealed strain: {revealedStrain.strainName}\n" +
                    $"Grade {grade} ({score}/1000) +${payout}";
            }

            if (newlyUnlocked)
            {
                harvestText.text += "\n STRAIN UNLOCKED FOR SHOP!";
            }
        }
        else
        {
            harvestText.text =
                $"Harvested {revealedStrain.strainName}! Grade {grade} ({score}/1000) +${payout}";
        }

        plantManager.DestroyCurrentPlant();

        SetupDropdown();
        RefreshUI();
        RefreshSeedListUI();
    }

    // ==========================
    // UI REFRESH
    // ==========================

    private void RefreshUI()
    {
        moneyText.text = $"Money: ${economyManager.Money}";
        dayText.text = $"Day: {timeManager.CurrentDay}";

        PlantInstance plant = plantManager.CurrentPlant;

        if (plant == null)
        {
            plantText.text = "Plant: NONE";
        }
        else
        {
            string shinyText = plant.seed.isShiny ? "YES" : "No";
            string strainName = plant.seed.isMysterySeed ? "???" : plant.strainData.strainName;

            plantText.text =
                $"Strain: {strainName}\n" +
                $"Rarity: {plant.seed.rarity}\n" +
                $"Shiny: {shinyText}\n" +
                $"Stage: {plant.stage}\n" +
                $"Growth: {plant.growthPercent:0}%\n" +
                $"Ripeness: {plant.ripenessPercent:0}%";
        }

        UpdateSelectedStrainUI();
    }

    public void PlantSpecificSeed(SeedInstance seed)
    {
        if (seed == null)
            return;

        if (plantManager.HasPlant)
        {
            harvestText.text = "A plant is already growing.";
            return;
        }

        // Remove the seed from inventory manually
        // We'll implement this cleanly next
        bool removed = seedInventory.RemoveSpecificSeed(seed);

        if (!removed)
        {
            harvestText.text = "Seed not found in inventory.";
            return;
        }

        plantManager.SpawnPlantFromSeed(seed);
        harvestText.text = $"Planted: {seed.DisplayName}";

        RefreshSeedListUI();
    }

    private void RefreshSeedListUI()
    {
        if (seedInventoryUI != null)
            seedInventoryUI.RefreshSafe();
    }
}