using System.Collections.Generic;
using UnityEngine;

public class DiscoveryManager : MonoBehaviour
{
    public static DiscoveryManager Instance;

    [Header("Database")]
    public StrainDatabase strainDatabase;

    private HashSet<string> discoveredStrainIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Debug.Log("DiscoveryManager Awake: " + gameObject.name);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsDiscovered(PlantStrainData strain)
    {
        if (strain == null) return false;
        return discoveredStrainIDs.Contains(strain.name);
    }

    public void DiscoverStrain(PlantStrainData strain)
    {
        if (strain == null) return;

        if (!discoveredStrainIDs.Contains(strain.name))
        {
            discoveredStrainIDs.Add(strain.name);
            Debug.Log($"DISCOVERED STRAIN: {strain.strainName}");

            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.SetStrainsUnlocked(GetDiscoveredCount());
            }
        }
    }

    public int GetDiscoveredCount()
    {
        return discoveredStrainIDs.Count;
    }

    public int GetTotalCount()
    {
        if (strainDatabase == null) return 0;
        return strainDatabase.strains.Count;
    }

    public List<PlantStrainData> GetAllStrains()
    {
        if (strainDatabase == null) return new List<PlantStrainData>();
        return strainDatabase.strains;
    }

    public List<PlantStrainData> GetDiscoveredStrains()
    {
        List<PlantStrainData> result = new List<PlantStrainData>();

        if (strainDatabase == null || strainDatabase.strains == null)
            return result;

        foreach (var strain in strainDatabase.strains)
        {
            if (strain == null) continue;

            if (discoveredStrainIDs.Contains(strain.name))
                result.Add(strain);
        }

        return result;
    }

    public List<string> GetDiscoveredStrainIDs()
    {
        return new List<string>(discoveredStrainIDs);
    }

    public void LoadDiscoveredStrainIDs(List<string> ids)
    {
        discoveredStrainIDs.Clear();

        if (ids == null)
            return;

        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(id))
                discoveredStrainIDs.Add(id);
        }

        // Update player stats count after loading
        if (PlayerStatsManager.Instance != null)
            PlayerStatsManager.Instance.SetStrainsUnlocked(GetDiscoveredCount());
    }
}