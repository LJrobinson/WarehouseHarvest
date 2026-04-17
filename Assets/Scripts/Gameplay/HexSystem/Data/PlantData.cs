using System.Collections.Generic;
using UnityEngine;

namespace Vertigro.Data
{
    // --- ENUMS ---
    // By matching the exact order of the matrix, NitrogenFixer = Row/Col 0, ShadeMaker = Row/Col 7 
    public enum PlantTag
    {
        NitrogenFixer = 0,
        HeavyFeeder = 1,
        Rooter = 2,
        Greens = 3,
        FruitBearer = 4,
        Herb = 5,
        Flower = 6,
        ShadeMaker = 7
    }

    // --- SCRIPTABLE OBJECT ---
    // This allows you to right-click in your Unity Project folder and create a new "Plant" data file.
    [CreateAssetMenu(fileName = "New Plant Data", menuName = "Vertigro/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("Base Stats")]
        [Tooltip("The name of the crop displayed to the player.")]
        public string plantName = "Generic Crop";

        [Tooltip("How many 'days' or turns it takes for this plant to be ready.")]
        public int daysToMature = 5;

        [Tooltip("The base value of the plant before any adjacency bonuses are calculated.")]
        public int baseHarvestScore = 10;

        [Header("Plant Identity")]
        [Tooltip("Assign any of the 8 tags that apply to this specific crop.")]
        public List<PlantTag> myTags = new List<PlantTag>();

       
        //[Header("Adjacency Rules")]
        //[Tooltip("List the bonuses or penalties this plant gets from its neighbors.")]
    }
}