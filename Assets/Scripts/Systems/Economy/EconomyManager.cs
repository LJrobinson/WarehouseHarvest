using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int Money { get; private set; } = 10000;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        Debug.Log($"Money added: {amount}. New total: {Money}");

        if (PlayerStatsManager.Instance != null)
            PlayerStatsManager.Instance.AddMoneyEarned(amount);
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount)
        {
            Debug.Log($"Not enough money to spend {amount}. Current: {Money}");
            return false;
        }

        Money -= amount;
        Debug.Log($"Money spent: {amount}. New total: {Money}");

        if (PlayerStatsManager.Instance != null)
            PlayerStatsManager.Instance.AddMoneySpent(amount);

        return true;
    }

    public void SetMoney(int amount)
    {
        Money = amount;
        Debug.Log($"Money set to: {Money}");
    }
}