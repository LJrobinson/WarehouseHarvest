using System;
using UnityEngine;

[Serializable]
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
            string shinyText = isShiny ? " Shiny " : "";
            string strainName = isMysterySeed ? "???" : strain.strainName;
            return $"{shinyText}{strainName} ({rarity})";
        }
    }
}