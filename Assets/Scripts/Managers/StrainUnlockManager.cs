using System.Collections.Generic;
using UnityEngine;

public class StrainUnlockManager : MonoBehaviour
{
    private Dictionary<PlantStrainData, StrainUnlockState> unlockStates =
        new Dictionary<PlantStrainData, StrainUnlockState>();

    public StrainUnlockState GetState(PlantStrainData strain)
    {
        if (strain == null)
            return StrainUnlockState.Unknown;

        if (!unlockStates.ContainsKey(strain))
            return StrainUnlockState.Unknown;

        return unlockStates[strain];
    }

    public bool IsDiscovered(PlantStrainData strain)
    {
        return GetState(strain) != StrainUnlockState.Unknown;
    }

    public bool IsUnlocked(PlantStrainData strain)
    {
        return GetState(strain) == StrainUnlockState.Unlocked;
    }

    public bool DiscoverStrain(PlantStrainData strain)
    {
        if (strain == null)
            return false;

        if (!unlockStates.ContainsKey(strain))
        {
            unlockStates[strain] = StrainUnlockState.Discovered;
            Debug.Log($"Discovered new strain: {strain.strainName}");
            return true; // newly discovered
        }

        return false;
    }

    public bool UnlockStrain(PlantStrainData strain)
    {
        if (strain == null)
            return false;

        StrainUnlockState current = GetState(strain);

        if (current == StrainUnlockState.Unlocked)
            return false;

        unlockStates[strain] = StrainUnlockState.Unlocked;
        Debug.Log($"Unlocked strain: {strain.strainName}");
        return true;
    }
}