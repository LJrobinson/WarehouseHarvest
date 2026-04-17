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

            if (currentNode.currentPlant != null && currentNode.currentPlant.seed != null)
                plantText.text = currentNode.currentPlant.seed.DisplayName;
            else if (currentNode.currentPlant != null)
                plantText.text = "Placed Plant";
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
