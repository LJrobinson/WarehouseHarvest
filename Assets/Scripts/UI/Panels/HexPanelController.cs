using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Vertigro.Logic
{
    public class HexPanelController : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text seedsText;
        [SerializeField] private TMP_Text productsText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text plantText;
        [SerializeField] private TMP_Text insertText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private Button harvestButton;
        [SerializeField] private Button plantModeButton;
        [SerializeField] private Button insertModeButton;
        [SerializeField] private Button sellAllButton;
        

        [Header("Gameplay References")]
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private SeedInventory seedInventory;
        [SerializeField] private HexSelectionController selectionController;
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
                SetHarvestButtonInteractable(false);
                SetText(stateText, "ERROR: No Selection Controller");
                return;
            }

            if (selectionController != null && modeText != null)
                modeText.text = $"Mode: {selectionController.CurrentPlacementMode}";

            if (economyManager != null)
                moneyText.text = $"${economyManager.Money}";

            if (seedInventory != null)
                seedsText.text = $"Seeds: {seedInventory.GetAllSeeds().Count}";

            if (productInventory != null)
                productsText.text = $"Products: {productInventory.GetTotalItems()}";

            currentNode = selectionController.SelectedNode;
            SetHarvestButtonInteractable(CanHarvest(currentNode));

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
            
            if (sellAllButton != null && productInventory != null)
            {
                sellAllButton.interactable = productInventory.GetTotalItems() > 0;
                sellAllButton.GetComponentInChildren<TMP_Text>().text = $"Sell All (${productInventory.GetEstimatedTotalValue()})";
            }
            
            RefreshModeButtons();
        }

        private static bool CanHarvest(HexNode node)
        {
            PlantInstance plant = node != null ? node.currentPlant : null;
            return plant != null && (plant.IsHarvestable || plant.IsOverripe);
        }

        private void SetHarvestButtonInteractable(bool interactable)
        {
            if (harvestButton != null)
                harvestButton.interactable = interactable;
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

        private void RefreshModeButtons()
        {
            if (selectionController == null)
                return;

            var mode = selectionController.CurrentPlacementMode;

            Color activeColor = Color.green;
            Color inactiveColor = Color.white;

            if (plantModeButton != null)
            {
                var colors = plantModeButton.colors;
                colors.normalColor = (mode == HexSelectionController.PlacementMode.Plant) ? activeColor : inactiveColor;
                plantModeButton.colors = colors;
            }

            if (insertModeButton != null)
            {
                var colors = insertModeButton.colors;
                colors.normalColor = (mode == HexSelectionController.PlacementMode.Insert) ? activeColor : inactiveColor;
                insertModeButton.colors = colors;
            }
        }

        public void SellAllProducts()
        {
            if (productInventory == null)
            {
                Debug.LogWarning("No ProductInventory assigned.");
                return;
            }

            if (economyManager == null)
            {
                Debug.LogWarning("No EconomyManager assigned.");
                return;
            }

            int totalItems = productInventory.GetTotalItems();

            if (totalItems == 0)
            {
                Debug.Log("No products to sell.");
                return;
            }

            int totalValue = productInventory.SellAll(economyManager);

            Debug.Log($"Sold {totalItems} items for ${totalValue}");

            Refresh();
        }
    }
}
