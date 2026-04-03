using System.Collections.Generic;
using UnityEngine;

public class DiscoveryManager : MonoBehaviour
{
    public HashSet<string> discoveredStrains = new HashSet<string>();

    public bool IsDiscovered(PlantStrainData strain)
    {
        if (strain == null) return false;
        return discoveredStrains.Contains(strain.strainName);
    }

    public void DiscoverStrain(PlantStrainData strain)
    {
        if (strain == null) return;

        if (!discoveredStrains.Contains(strain.strainName))
        {
            discoveredStrains.Add(strain.strainName);
            Debug.Log($"DISCOVERED STRAIN: {strain.strainName}");
        }
    }
}