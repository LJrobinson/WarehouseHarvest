using System.Collections.Generic;

[System.Serializable]
public class SeedInventorySummary
{
    public PlantStrainData strain;
    public bool isMystery;

    public int totalCount;

    public Dictionary<SeedRarity, int> rarityCounts = new Dictionary<SeedRarity, int>();

    public int shinyCount;

    public string DisplayName
    {
        get
        {
            if (isMystery)
                return $"??? Bagseed x{totalCount}";

            return $"{strain.strainName} x{totalCount}";
        }
    }
}