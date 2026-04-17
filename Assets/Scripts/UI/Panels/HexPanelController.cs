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
        [SerializeField] private PlantInstance plantPrefab;

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

            // Keep panel synced to selection
            if (selectionController != null && selectionController.SelectedNode != currentNode)
            {
                currentNode = selectionController.SelectedNode;
                Refresh();
            }
        }

        public void Refresh()
        {
            if (selectionController == null)
            {
                stateText.text = "ERROR: No Selection Controller";
                return;
            }

            currentNode = selectionController.SelectedNode;

            if (currentNode == null)
            {
                titleText.text = "No Hex Selected";
                plantText.text = "-";
                insertText.text = "-";
                stateText.text = "NONE";
                return;
            }

            titleText.text = $"HEX: {currentNode.hexCoords}";

            if (currentNode.plantedSeed != null)
                plantText.text = currentNode.plantedSeed.DisplayName;
            else
                plantText.text = "Empty";

            if (currentNode.currentInsert != null)
                insertText.text = currentNode.currentInsert.insertName;
            else
                insertText.text = "None";

            stateText.text = currentNode.IsEmpty ? "EMPTY" : "OCCUPIED";
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

            // Remove from inventory FIRST
            if (!seedInventory.RemoveSpecificSeed(seedToPlant))
            {
                Debug.Log("Failed to remove seed from inventory.");
                return;
            }

            currentNode.TryPlantSeed(seedToPlant, plantPrefab);

            Refresh();
        }
    }
}