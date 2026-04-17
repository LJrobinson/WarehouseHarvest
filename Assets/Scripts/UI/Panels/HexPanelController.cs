using UnityEngine;
using TMPro;

namespace Vertigro.Logic
{
    public class HexPanelController : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text plantText;
        [SerializeField] private TMP_Text insertText;
        [SerializeField] private TMP_Text stateText;

        [Header("Gameplay References")]
        [SerializeField] private HexSelectionController selectionController;
        [SerializeField] private SeedInventory seedInventory;
        [SerializeField] private ProductInventory productInventory;
        [SerializeField] private GameObject plantPrefab;

        private HexNode currentNode;

        public override void Open()
        {
            base.Open();
            Refresh();
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            Refresh();
        }

        public void Refresh()
        {
            if (selectionController == null)
            {
                SetText(stateText, "ERROR: No Selection Controller");
                return;
            }

            currentNode = selectionController.SelectedNode;

            if (currentNode == null)
            {
                SetText(titleText, "No Hex Selected");
                SetText(plantText, "-");
                SetText(insertText, "-");
                SetText(stateText, "NONE");
                return;
            }

            SetText(titleText, $"HEX: {currentNode.hexCoords}");

            if (currentNode.currentPlant != null && currentNode.currentPlant.seed != null)
                SetText(plantText, currentNode.currentPlant.seed.DisplayName);
            else if (currentNode.currentPlant != null)
                SetText(plantText, "Placed Plant");
            else
                SetText(plantText, "Empty");

            if (currentNode.currentInsert != null)
                SetText(insertText, currentNode.currentInsert.insertName);
            else
                SetText(insertText, "None");

            SetText(stateText, GetNodeStateText(currentNode));
        }

        private static string GetNodeStateText(HexNode node)
        {
            if (node == null || node.IsEmpty)
                return "EMPTY";

            PlantInstance plant = node.currentPlant;

            if (plant == null)
                return "OCCUPIED";

            if (plant.seed == null || plant.strainData == null)
                return "PLANT\nNo seed/strain data";

            return $"PLANT\nStage: {plant.stage}\nGrowth: {plant.growthPercent:0}%\nRipeness: {plant.ripenessPercent:0}%";
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
                text.text = value;
        }

        // Hook this to a button
        public void HarvestSelectedPlant()
        {
            if (selectionController == null)
            {
                Debug.LogWarning("HexPanelController: No Selection Controller assigned.");
                return;
            }

            currentNode = selectionController.SelectedNode;

            if (currentNode == null)
            {
                Debug.Log("Cannot harvest: no hex selected.");
                return;
            }

            PlantInstance plant = currentNode.currentPlant;

            if (plant == null)
            {
                Debug.Log("Cannot harvest: selected hex has no plant.");
                Refresh();
                return;
            }

            if (!plant.IsHarvestable && !plant.IsOverripe)
            {
                Debug.Log("Cannot harvest: plant is not ready.");
                Refresh();
                return;
            }

            if (productInventory == null)
            {
                Debug.LogWarning("HexPanelController: No ProductInventory assigned.");
                return;
            }

            HarvestProductInstance product = HarvestProcessor.CreateHarvestProduct(plant);

            if (product == null)
            {
                Debug.LogWarning("Harvest failed: plant could not create product.");
                Refresh();
                return;
            }

            productInventory.AddProduct(product);
            currentNode.RemovePlant();

            Debug.Log($"Harvested hex plant: {product.DisplayName}");
            Refresh();
        }

        // Hook this to a button
        public void PlantFirstSeedInInventory()
        {
            if (currentNode == null)
                return;

            if (!currentNode.IsEmpty)
            {
                Debug.Log("Cannot plant: hex is not empty.");
                return;
            }

            if (seedInventory == null)
            {
                Debug.LogWarning("HexPanelController: No SeedInventory assigned.");
                return;
            }

            var seeds = seedInventory.GetAllSeeds();

            if (seeds == null || seeds.Count == 0)
            {
                Debug.Log("No seeds in inventory.");
                return;
            }

            SeedInstance seedToPlant = seeds[0];

            if (!currentNode.TryPlantSeed(seedToPlant, plantPrefab))
            {
                Debug.Log("Failed to plant seed.");
                return;
            }

            if (!seedInventory.RemoveSpecificSeed(seedToPlant))
                Debug.LogWarning("HexPanelController: Planted seed, but failed to remove it from inventory.");

            Refresh();
        }
    }
}
