using System;
using UnityEngine;

[System.Serializable]
public class StrainStatsData
{
    public string strainName;

    public bool firstHarvestComplete;
    public string firstHarvestDate;

    public int totalHarvests;

    public int moneyEarned;
    public int moneySpentOnSeeds;

    public void RecordHarvest()
    {
        totalHarvests++;

        if (!firstHarvestComplete)
        {
            firstHarvestComplete = true;
            firstHarvestDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }

    public void AddMoneyEarned(int amount)
    {
        moneyEarned += amount;
    }

    public void AddMoneySpent(int amount)
    {
        moneySpentOnSeeds += amount;
    }
}