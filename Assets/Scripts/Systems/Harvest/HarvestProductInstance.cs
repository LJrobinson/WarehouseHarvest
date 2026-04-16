using System;

[System.Serializable]
public class HarvestProductInstance
{
    public string strainName;
    public SeedRarity rarity;
    public bool isShiny;

    public int gradeScore;
    public string gradeLetter;

    public float grams;

    // This is NOT market price yet, just base strain value
    public int baseValuePerGram;

    public int TotalValue
    {
        get
        {
            return (int)(grams * baseValuePerGram);
        }
    }

    public string DisplayName
    {
        get
        {
            string shinyText = isShiny ? "✨ " : "";
            return $"{shinyText}{strainName} ({rarity}) [{gradeLetter}] - {grams:0.0}g";
        }
    }
}