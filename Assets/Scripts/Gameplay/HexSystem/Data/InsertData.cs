using System.Collections.Generic;
using UnityEngine;

namespace Vertigro.Data
{
    // --- ENUMS ---
    // A predefined list of equipment types so we don't have to type them manually.
    public enum InsertType
    {
        Heater,
        Humidifier,
        GrowLight,
        NutrientDrip,
        Fan
    }

    // --- SCRIPTABLE OBJECT ---
    // This allows you to right-click in Unity and create a new piece of table equipment.
    [CreateAssetMenu(fileName = "New Insert Data", menuName = "Vertigro/Insert Data")]
    public class InsertData : ScriptableObject
    {
        [Header("Basic Equipment Info")]
        [Tooltip("The name of the equipment displayed to the player.")]
        public string insertName = "New Equipment";

        [Tooltip("What category of equipment is this?")]
        public InsertType equipmentType;

        [Header("Area of Effect Rules")]
        [Tooltip("How much does this insert increase (or decrease) the harvest score of neighboring hexes?")]
        public int adjacencyBonus = 2; // Default to a +2 bonus, but the Inspector can override this.

        [Tooltip("Which specific plant tags actually benefit from this insert? (e.g., A Humidifier might only buff 'Thirsty' plants).")]
        public List<PlantTag> affectedTags = new List<PlantTag>();

        // REMINDER FOR NON-ENGINEERS:
        // Unlike plants, Inserts do not have "Days To Mature" or a "Base Harvest Score".
        // They are static objects placed on the hex grid to boost the plants around them.
    }
}