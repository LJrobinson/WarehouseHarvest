using System.Collections.Generic;
using UnityEngine;

public class StrainStatsManager : MonoBehaviour
{
    public static StrainStatsManager Instance;

    [SerializeField]
    private List<StrainStatsData> statsList = new List<StrainStatsData>();

    private Dictionary<string, StrainStatsData> lookup = new Dictionary<string, StrainStatsData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RebuildLookup();
    }

    private void RebuildLookup()
    {
        lookup.Clear();

        foreach (var stat in statsList)
        {
            if (stat == null || string.IsNullOrEmpty(stat.strainName))
                continue;

            lookup[stat.strainName] = stat;
        }
    }

    private StrainStatsData GetOrCreate(string strainName)
    {
        if (string.IsNullOrEmpty(strainName))
            strainName = "UNKNOWN";

        if (!lookup.ContainsKey(strainName))
        {
            StrainStatsData newStats = new StrainStatsData()
            {
                strainName = strainName
            };

            statsList.Add(newStats);
            lookup[strainName] = newStats;
        }

        return lookup[strainName];
    }

    public StrainStatsData GetStats(string strainName)
    {
        return GetOrCreate(strainName);
    }

    public void RecordSeedPurchase(string strainName, int moneySpent)
    {
        var stats = GetOrCreate(strainName);
        stats.AddMoneySpent(moneySpent);
    }

    public void RecordHarvest(string strainName)
    {
        var stats = GetOrCreate(strainName);
        stats.RecordHarvest();
    }

    public void RecordMoneyEarned(string strainName, int amount)
    {
        var stats = GetOrCreate(strainName);
        stats.AddMoneyEarned(amount);
    }

    public List<StrainStatsData> GetAllStats()
    {
        return statsList;
    }
}