using System.Collections.Generic;
using UnityEngine;
using Vertigro.Data;

namespace Vertigro.Logic
{
    public class HexNode : MonoBehaviour
    {
        public Vector3Int hexCoords;
        public int floorLevel;

        [Header("Current Occupant")]
        [HideInInspector]
        public PlantInstance currentPlant;
        public InsertData currentInsert;

        [Header("State")]
        public int growthProgress = 0;

        public bool isOccupied => currentPlant != null || currentInsert != null;
        public bool IsEmpty => currentPlant == null && currentInsert == null;

        public List<HexNode> neighbors = new List<HexNode>();

        [Header("Seed System")]
        public SeedInstance plantedSeed;

        public bool TryPlaceInsert(InsertData insert)
        {
            if (insert == null) return false;
            if (!IsEmpty) return false;

            currentInsert = insert;
            return true;
        }

        public bool TryPlacePlant(GameObject plantPrefab)
        {
            return TryPlacePlant(plantPrefab, null);
        }

        public bool TryPlacePlant(PlantInstance plantPrefab)
        {
            if (plantPrefab == null) return false;

            return TryPlacePlant(plantPrefab.gameObject, null);
        }

        public bool TryPlantSeed(SeedInstance seed, GameObject plantPrefab)
        {
            if (seed == null) return false;

            return TryPlacePlant(plantPrefab, seed);
        }

        public bool TryPlantSeed(SeedInstance seed, PlantInstance plantPrefab)
        {
            if (seed == null || plantPrefab == null) return false;

            return TryPlacePlant(plantPrefab.gameObject, seed);
        }

        private bool TryPlacePlant(GameObject plantPrefab, SeedInstance seed)
        {
            if (plantPrefab == null) return false;
            if (!IsEmpty) return false;

            GameObject plantObject = Instantiate(plantPrefab, transform);
            plantObject.transform.localPosition = Vector3.zero;

            PlantInstance plant = plantObject.GetComponent<PlantInstance>();

            if (plant == null)
                plant = plantObject.GetComponentInChildren<PlantInstance>(true);

            if (plant == null)
            {
                Debug.LogWarning($"HexNode: Plant prefab '{plantPrefab.name}' does not contain a PlantInstance component.");
                Destroy(plantObject);
                return false;
            }

            if (!plantObject.activeSelf)
                plantObject.SetActive(true);

            if (seed != null)
                plant.InitializeFromSeed(seed);

            currentPlant = plant;
            plantedSeed = plant.seed;
            return true;
        }

        public void Interact()
        {
            Debug.Log($"HexNode Interact: {name}");

            if (UIManager.Instance != null)
                UIManager.Instance.OpenPanel("HexPanel");
        }

        public void ProcessTick()
        {
            if (currentPlant == null || currentPlant.seed == null)
                return;

            growthProgress++;

            var strain = currentPlant.seed.EffectiveStrain;
            if (strain == null) return;

            if (growthProgress >= strain.growthPerDay)
            {
                Debug.Log($"{strain.strainName} at {hexCoords} is ready for harvest!");
            }
        }

        public void FindNeighbors(List<HexNode> allNodes)
        {
            neighbors.Clear();
            foreach (var n in allNodes)
                if (IsAdjacent(n)) neighbors.Add(n);
        }

        private bool IsAdjacent(HexNode other)
        {
            if (other.floorLevel == floorLevel)
            {
                float d =
                    (Mathf.Abs(hexCoords.x - other.hexCoords.x) +
                     Mathf.Abs(hexCoords.y - other.hexCoords.y) +
                     Mathf.Abs(hexCoords.z - other.hexCoords.z)) / 2;

                return d == 1;
            }

            return other.hexCoords == hexCoords &&
                   Mathf.Abs(other.floorLevel - floorLevel) == 1;
        }

        public int CalculateAdjacencyScore()
        {
            if (currentPlant == null || currentPlant.seed == null)
                return 0;

            int total = 0;

            var myTags = currentPlant.seed.EffectiveStrain?.myTags;
            if (myTags == null) return 0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || !neighbor.isOccupied)
                    continue;

                // plant vs plant
                if (neighbor.currentPlant != null && neighbor.currentPlant.seed != null)
                {
                    var neighborTags = neighbor.currentPlant.seed.EffectiveStrain?.myTags;
                    if (neighborTags != null)
                    {
                        foreach (var a in myTags)
                        foreach (var b in neighborTags)
                            total += AdjacencyDictionary.GetBonus(a, b);
                    }
                }

                // insert effects
                if (neighbor.currentInsert != null)
                {
                    foreach (var a in myTags)
                    foreach (var b in neighbor.currentInsert.affectedTags)
                        total += InsertDictionary.GetAdjacencyBonus(a, b);
                }
            }

            return total;
        }

        public string GetDebugState()
        {
            if (currentPlant == null && currentInsert == null) return "EMPTY";
            if (currentPlant != null) return $"PLANT: {currentPlant.seed?.EffectiveStrain?.strainName}";
            return $"INSERT: {currentInsert.name}";
        }
    }
}
