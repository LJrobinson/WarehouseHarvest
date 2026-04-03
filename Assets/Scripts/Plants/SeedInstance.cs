[System.Serializable]
public class SeedInstance
{
    public PlantStrainData strain;
    public SeedRarity rarity;
    public bool isShiny;
    public bool isMysterySeed;
    public int geneticsBonus;

    public string DisplayName
    {
        get
        {
            string strainName = isMysterySeed ? "??? Bagseed" : strain != null ? strain.strainName : "Unknown";

            string shinyText = isShiny ? " Shiny " : "";

            return $"{strainName} ({rarity}){shinyText}";
        }
    }
}