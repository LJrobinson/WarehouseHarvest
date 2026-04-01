using UnityEngine;

public class PackLoyaltyManager : MonoBehaviour
{
    public int twentyPackStreak = 0;

    public void RegisterTwentyPackPurchase()
    {
        twentyPackStreak++;
    }

    public bool HasEarnedBonus()
    {
        return twentyPackStreak >= 5;
    }

    public void ResetStreak()
    {
        twentyPackStreak = 0;
    }
}