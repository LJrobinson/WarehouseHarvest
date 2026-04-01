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

    private PlantStrainData selectedStrain;

    private void Start()
    {
        SetupDropdown();
        RefreshUI();
    }

    private void SetupDropdown()
    {
        if (strainDatabase == null || strainDatabase.strains.Count == 0)
        {
            Debug.LogError("No strainDatabase assigned or no strains in database!");
            return;
        }

        strainDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (PlantStrainData strain in strainDatabase.strains)
        {
            options.Add(strain.strainName);
        }

        strainDropdown.AddOptions(options);

        strainDropdown.onValueChanged.AddListener(OnDropdownChanged);

        // Default selection
        selectedStrain = strainDatabase.strains[0];
        UpdateSelectedStrainUI();
    }

    private void OnDropdownChanged(int index)
    {
        if (strainDatabase == null) return;

        selectedStrain = strainDatabase.strains[index];
        UpdateSelectedStrainUI();
    }

    private void UpdateSelectedStrainUI()
    {
        if (selectedStrain == null)
            return;

        strainInfoText.text =
            $"{selectedStrain.strainName}\n" +
            $"Cost: ${selectedStrain.seedCost}\n" +
            $"5-Pack: ${selectedStrain.pack5Cost}\n" +
            $"20-Pack: ${selectedStrain.pack20Cost}\n" +
            $"Growth/Day: {selectedStrain.growthPerDay}\n" +
            $"Ripeness/Day: {selectedStrain.ripenessPerDayInFlower}\n" +
            $"Harvest Window: {selectedStrain.harvestWindowStart}-{selectedStrain.harvestWindowEnd}\n" +
            $"Payout Mult: x{selectedStrain.payoutMultiplier:0.00}";

        int ownedSeeds = seedInventory.GetSeedCount(selectedStrain);
        seedCountText.text = $"Seeds Owned: {ownedSeeds}";
    }

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
    }

    public void Buy5PackButton()
    {
        BuySeedPack(5, selectedStrain.pack5Cost, 0.03f);
    }

    public void Buy20PackButton()
    {
        BuySeedPack(20, selectedStrain.pack20Cost, 0.06f);
    }

    private void BuySeedPack(int amount, int packCost, float rarityBoost)
    {
        if (selectedStrain == null)
            return;

        bool success = economyManager.SpendMoney(packCost);

        if (!success)
        {
            harvestText.text = "Not enough money to buy pack.";
            return;
        }

        int shinyCount = 0;
        int rarePlusCount = 0;

        for (int i = 0; i < amount; i++)
        {
            SeedInstance seed = SeedGenerator.GenerateSeed(selectedStrain, rarityBoost);
            seedInventory.AddSeed(seed);

            if (seed.isShiny)
                shinyCount++;

            if (seed.rarity == SeedRarity.Rare || seed.rarity == SeedRarity.Epic || seed.rarity == SeedRarity.Legendary)
                rarePlusCount++;
        }

        harvestText.text = $"Bought {amount}-Pack: {rarePlusCount} Rare+ / {shinyCount} Shiny";

        RefreshUI();
    }

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

        RefreshUI();
    }

    public void AddMoneyButton()
    {
        economyManager.AddMoney(5000);
        RefreshUI();
    }

    public void SpendMoneyButton()
    {
        economyManager.SpendMoney(25);
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

        harvestText.text = $"Harvested {plant.strainData.strainName}! Grade {grade} ({score}/1000) +${payout}";

        plantManager.DestroyCurrentPlant();

        RefreshUI();
    }

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

            plantText.text =
                $"Strain: {plant.strainData.strainName}\n" +
                $"Rarity: {plant.seed.rarity}\n" +
                $"Shiny: {shinyText}\n" +
                $"Stage: {plant.stage}\n" +
                $"Growth: {plant.growthPercent:0}%\n" +
                $"Ripeness: {plant.ripenessPercent:0}%";
        }

        UpdateSelectedStrainUI();
    }
}