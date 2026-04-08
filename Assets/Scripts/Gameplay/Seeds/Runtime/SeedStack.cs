using System.Collections.Generic;

[System.Serializable]
public class SeedStack
{
    public PlantStrainData strain;
    public SeedRarity rarity;
    public bool isMystery;

    public int count;

    public string DisplayName
    {
        get
        {
            if (isMystery)
                return $"??? Bagseed ({rarity}) x{count}";

            string name = strain != null ? strain.strainName : "Unknown";
            return $"{name} ({rarity}) x{count}";
        }
    }
}