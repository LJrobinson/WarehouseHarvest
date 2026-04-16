using System.Collections.Generic;
using UnityEngine;
using Vertigrow.Data; // Access our plant/insert definitions

namespace Vertigrow.Logic
{
    public class HexNode : MonoBehaviour
    {
        public Vector3Int hexCoords;
        public int floorLevel;

        [Header("Current Occupant")]
        public PlantData currentPlant;    // The "blueprint" for the plant currently growing here
        public InsertData currentInsert;  // The "blueprint" for a piece of equipment here

        [Header("State")]
        public int growthProgress = 0;    // How many days this plant has been growing
        public bool isOccupied => currentPlant != null || currentInsert != null;

        public List<HexNode> neighbors = new List<HexNode>();

        /// <summary>
        /// This is called by the TableGenerator every "Next Day".
        /// It handles the actual logic of growing the plant.
        /// </summary>
        public void ProcessTick()
        {
            // If there is no plant here, there's nothing to grow!
            if (currentPlant == null) return;

            // Increment growth progress
            growthProgress++;

            if (growthProgress >= currentPlant.daysToMature)
            {
                Debug.Log($"{currentPlant.plantName} at {hexCoords} is ready for harvest!");
                // Future: Trigger harvest score calculation here
            }
        }

        // --- ADJACENCY LOGIC ---
        public void FindNeighbors(List<HexNode> allNodes)
        {
            neighbors.Clear();
            foreach (var potentialNeighbor in allNodes)
            {
                if (IsAdjacent(potentialNeighbor)) neighbors.Add(potentialNeighbor);
            }
        }

        private bool IsAdjacent(HexNode other)
        {
            if (other.floorLevel == this.floorLevel)
            {
                float d = (Mathf.Abs(hexCoords.x - other.hexCoords.x) +
                           Mathf.Abs(hexCoords.y - other.hexCoords.y) +
                           Mathf.Abs(hexCoords.z - other.hexCoords.z)) / 2;
                return d == 1;
            }
            // Vertical adjacency (Same spot, floor above/below)
            return (other.hexCoords == this.hexCoords && Mathf.Abs(other.floorLevel - this.floorLevel) == 1);
        }

        /// <summary>
        /// Looks at all neighbors and calculates the final +/- adjacency modifier from BOTH plants and inserts.
        /// </summary>
        public int CalculateAdjacencyScore()
        {
            if (currentPlant == null) return 0;

            int totalModifier = 0;

            foreach (HexNode neighbor in neighbors)
            {
                if (!neighbor.isOccupied) continue;

                // 1. Check Plant-to-Plant Bonuses
                if (neighbor.currentPlant != null)
                {
                    foreach (PlantTag myTag in currentPlant.myTags)
                    {
                        foreach (PlantTag neighborTag in neighbor.currentPlant.myTags)
                        {
                            totalModifier += AdjacencyDictionary.GetBonus(myTag, neighborTag);
                        }
                    }
                }

                // 2. Check Insert-to-Plant Bonuses (Equipment like ShadeMakers)
                if (neighbor.currentInsert != null)
                {
                    foreach (PlantTag myTag in currentPlant.myTags)
                    {
                        // Assuming your InsertData blueprint also uses the PlantTag enum (e.g., Tag = ShadeMaker)
                        foreach (PlantTag insertTag in neighbor.currentInsert.affectedTags) 
                        {
                            totalModifier += InsertDictionary.GetAdjacencyBonus(myTag, insertTag);
                        }
                    }
                }
            }

            return totalModifier;
        }
    }
}