using System;
using UnityEngine;

[Serializable]
public class SeedInstance
{
    public PlantStrainData strain;
    public SeedRarity rarity;
    public bool isShiny;

    public int geneticsBonus;

    public string DisplayName
    {
        get
        {
            string shinyText = isShiny ? "Shiny" : "";
            return $"{shinyText}{strain.strainName} ({rarity})";
        }
    }
}