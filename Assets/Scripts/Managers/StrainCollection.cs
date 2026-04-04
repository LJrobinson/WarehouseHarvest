using System.Collections.Generic;
using UnityEngine;

public class StrainCollection : MonoBehaviour
{
    [Header("References")]
    public StrainDatabase strainDatabase;

    private HashSet<string> discoveredStrains = new HashSet<string>();

    public int TotalStrains => strainDatabase != null ? strainDatabase.strains.Count : 0;
    public int DiscoveredCount => discoveredStrains.Count;

    public float DiscoveryPercent
    {
        get
        {
            if (TotalStrains == 0) return 0f;
            return (float)DiscoveredCount / TotalStrains * 100f;
        }
    }

    public bool IsDiscovered(PlantStrainData strain)
    {
        if (strain == null) return false;
        return discoveredStrains.Contains(strain.strainName);
    }

    public bool DiscoverStrain(PlantStrainData strain)
    {
        if (strain == null) return false;

        bool added = discoveredStrains.Add(strain.strainName);

        if (added)
            Debug.Log($"[COLLECTION] New strain discovered: {strain.strainName}");

        return added;
    }

    public List<PlantStrainData> GetAllStrains()
    {
        if (strainDatabase == null)
            return new List<PlantStrainData>();

        return strainDatabase.strains;
    }
}