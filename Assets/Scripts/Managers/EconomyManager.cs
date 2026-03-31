using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int Money { get; private set; } = 1000;

    public void AddMoney(int amount)
    {
        Money += amount;
        Debug.Log($"Money added: {amount}. New total: {Money}");
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
        return true;
    }
}