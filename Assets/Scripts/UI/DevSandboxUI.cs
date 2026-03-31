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
        RefreshUI();
    }

    public void SpawnPlantButton()
    {
        plantManager.SpawnPlaceholderPlant();
    }

    private void RefreshUI()
    {
        moneyText.text = $"Money: ${economyManager.Money}";
        dayText.text = $"Day: {timeManager.CurrentDay}";
    }
}