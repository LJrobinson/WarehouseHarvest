using TMPro;
using UnityEngine;

public class DevSandboxUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private PlantManager plantManager;

    [Header("UI Text")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text plantText;
    [SerializeField] private TMP_Text harvestText;

    private void Start()
    {
        RefreshUI();
    }

    public void AddMoneyButton()
    {
        economyManager.AddMoney(50);
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

    public void SpawnPlantButton()
    {
        plantManager.SpawnPlaceholderPlant();
        harvestText.text = "";
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

        float multiplier = plant.strainData.payoutMultiplier;
        int payout = Mathf.RoundToInt(score * 0.5f * multiplier);

        economyManager.AddMoney(payout);

        harvestText.text = $"Harvested! Grade {grade} ({score}/1000) +${payout}";

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
            plantText.text =
                $"Strain: {plant.strainData.strainName}\n" +
                $"Stage: {plant.stage}\n" +
                $"Growth: {plant.growthPercent:0}%\n" +
                $"Ripeness: {plant.ripenessPercent:0}%";
        }
    }
}