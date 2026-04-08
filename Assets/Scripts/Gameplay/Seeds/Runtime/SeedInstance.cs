[System.Serializable]
public class SeedInstance
{
    public PlantStrainData strain;

    // Mystery bagseed mechanic
    public bool isMysterySeed = false;

    // Hidden true strain for mystery seeds
    public PlantStrainData hiddenStrain;

    public SeedRarity rarity;
    public bool isShiny;

    public int geneticsBonus = 0;

    // This is what plants should actually grow from
    public PlantStrainData EffectiveStrain
    {
        get
        {
            if (isMysterySeed && hiddenStrain != null)
                return hiddenStrain;

            return strain;
        }
    }

    public string DisplayName
    {
        get
        {
            if (isMysterySeed)
                return $"??? ({rarity})";

            if (strain == null)
                return $"Unknown ({rarity})";

            return $"{strain.strainName} ({rarity})";
        }
    }

    public void RevealMystery()
    {
        if (!isMysterySeed) return;
        if (hiddenStrain == null) return;

        strain = hiddenStrain;
        isMysterySeed = false;
    }
}