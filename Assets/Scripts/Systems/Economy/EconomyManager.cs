using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int Money { get; private set; } = 1000;

    public void AddMoney(int amount)
    {
        Money += amount;
        Debug.Log($"Money added: {amount}. New total: {Money}");
        PlayerStatsManager.Instance.AddMoneyEarned(amount);
        Debug.Log($"Money earned: {amount}");
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
        PlayerStatsManager.Instance.AddMoneySpent(amount);
        Debug.Log($"Money wasted: {amount}");
        return true;
    }

    public void SetMoney(int amount)
    {
        Money = amount;
        Debug.Log($"Money set to: {Money}");
    }
}