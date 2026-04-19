using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Vertigro.Logic
{
    public class HexPanelController : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text seedsText;
        [SerializeField] private TMP_Text seedSummaryText;
        [SerializeField] private TMP_Text nextSeedText;
        [SerializeField] private TMP_Text productsText;
        [SerializeField] private TMP_Text productSummaryText;
        [SerializeField] private TMP_Text rackSlotSummaryText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text plantText;
        [SerializeField] private TMP_Text insertText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private Button harvestButton;
        [SerializeField] private Button plantModeButton;
        [SerializeField] private Button insertModeButton;
        [SerializeField] private Button sellAllButton;
        [FormerlySerializedAs("upgradeRackButton")]
        [SerializeField] private Button upgradeShelfButton;
        [SerializeField] private Button unlockShelf2Button;
        [SerializeField] private Button unlockShelf6Button;

        [Header("Gameplay References")]
        [SerializeField] private DiscoveryManager discoveryManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private GameObject plantPrefab;
        [SerializeField] private ProductInventory productInventory;        
        [SerializeField] private SeedInventory seedInventory;
        [SerializeField] private HexSelectionController selectionController;
        [SerializeField] private TableController tableController;
        [SerializeField] private RackController rackController;

        [Header("Shelf Capacity Upgrade")]
        [FormerlySerializedAs("rackUpgradeCost")]
        [SerializeField] private int shelfUpgradeCost = 500;

        [Header("Rack Shelf Unlock")]
        [SerializeField] private int shelf2UnlockCost = 250;
        [SerializeField] private int shelf6UnlockCost = 250;
        
        private const int MaxSeedSummaryLines = 4;
        private const int MaxProductSummaryLines = 4;
        private const int UnlockShelf2SlotIndex = 2;
        private const int UnlockShelf6SlotIndex = 6;

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
                SetUpgradeButtonState();
                SetUnlockShelfButtonStates();
                RefreshRackSlotSummary();
                SetText(stateText, "ERROR: No Selection Controller");
                return;
            }

            if (modeText != null)
                modeText.text = $"Mode: {selectionController.CurrentPlacementMode}";

            if (economyManager != null)
                moneyText.text = $"Money: ${economyManager.Money}";

            if (seedInventory != null)
            {
                seedsText.text = $"Seeds: {seedInventory.GetAllSeeds().Count}";
                SetText(seedSummaryText, BuildSeedSummary(seedInventory.GetSeedStacks()));
                SetText(nextSeedText, GetNextSeedText());
            }
            else
            {
                SetText(seedsText, "Seeds: 0");
                SetText(seedSummaryText, "No seeds available");
                SetText(nextSeedText, "Next Seed: -");
            }

            if (productInventory != null)
            {
                productsText.text = $"Products: {productInventory.GetTotalItems()}";
                SetText(productSummaryText, BuildProductSummary(productInventory.GetAllProduct()));
            }
            else
            {
                SetText(productsText, "Products: 0");
                SetText(productSummaryText, "No products available");
            }

            currentNode = selectionController.SelectedNode;
            SetHarvestButtonInteractable(CanHarvest(currentNode));
            SetUpgradeButtonState();
            SetUnlockShelfButtonStates();
            RefreshRackSlotSummary();

            if (currentNode == null)
            {
                SetText(titleText, "No Hex Selected");
                SetText(plantText, "-");
                SetText(insertText, "-");
                SetText(stateText, "NONE");
                RefreshModeButtons();
                RefreshSellAllButton();
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
            
            RefreshSellAllButton();
            RefreshModeButtons();
        }

        private void RefreshRackSlotSummary()
        {
            SetText(rackSlotSummaryText, BuildRackSlotSummary(rackController));
        }

        private static string BuildRackSlotSummary(RackController rack)
        {
            if (rack == null)
                return "Rack Slots\nNo rack assigned";

            StringBuilder builder = new StringBuilder();
            builder.Append("Rack Slots");

            for (int i = 1; i <= RackController.ShelfSlotCount; i++)
            {
                ShelfSlotRecord slot = rack.GetShelfSlot(i);
                builder.AppendLine();
                builder.Append($"Shelf {i} - {GetShelfSlotStatus(slot)}");
            }

            return builder.ToString();
        }

        private static string GetShelfSlotStatus(ShelfSlotRecord slot)
        {
            if (slot == null || !slot.isUnlocked)
                return "Locked";

            return slot.shelf != null ? "Active" : "Unlocked";
        }

        private void RefreshSellAllButton()
        {
            if (sellAllButton != null && productInventory != null)
            {
                sellAllButton.interactable = productInventory.GetTotalItems() > 0;

                TMP_Text sellAllLabel = sellAllButton.GetComponentInChildren<TMP_Text>();
                if (sellAllLabel != null)
                    sellAllLabel.text = $"Sell All (${productInventory.GetEstimatedTotalValue()})";
            }
        }

        private void SetUpgradeButtonState()
        {
            if (upgradeShelfButton == null)
                return;

            bool isMaxed = tableController != null && tableController.IsMaxShelfLevelReached;
            upgradeShelfButton.interactable = !isMaxed;

            TMP_Text upgradeLabel = upgradeShelfButton.GetComponentInChildren<TMP_Text>();
            if (upgradeLabel != null)
            {
                upgradeLabel.text = isMaxed
                    ? "Shelf Maxed"
                    : $"Upgrade Shelf (${shelfUpgradeCost})";
            }
        }

        private void SetUnlockShelfButtonStates()
        {
            SetUnlockShelfButtonState(unlockShelf2Button, UnlockShelf2SlotIndex, shelf2UnlockCost);
            SetUnlockShelfButtonState(unlockShelf6Button, UnlockShelf6SlotIndex, shelf6UnlockCost);
        }

        private void SetUnlockShelfButtonState(Button unlockButton, int slotIndex, int unlockCost)
        {
            if (unlockButton == null)
                return;

            ShelfSlotRecord slot = rackController != null ? rackController.GetShelfSlot(slotIndex) : null;
            bool isUnlocked = slot != null && slot.isUnlocked;
            bool canAfford = economyManager != null && economyManager.Money >= unlockCost;
            bool hasValidCost = unlockCost >= 0;

            unlockButton.interactable = rackController != null && slot != null && !isUnlocked && canAfford && hasValidCost;

            TMP_Text unlockLabel = unlockButton.GetComponentInChildren<TMP_Text>();
            if (unlockLabel != null)
            {
                if (isUnlocked)
                    unlockLabel.text = slot.shelf != null ? $"Shelf {slotIndex} Active" : $"Shelf {slotIndex} Unlocked";
                else
                    unlockLabel.text = $"Unlock Shelf {slotIndex} (${unlockCost})";
            }
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

        private static string BuildSeedSummary(List<SeedStack> seedStacks)
        {
            if (seedStacks == null || seedStacks.Count == 0)
                return "No seeds available";

            StringBuilder builder = new StringBuilder();
            int shownCount = 0;

            foreach (SeedStack stack in seedStacks)
            {
                if (stack == null)
                    continue;

                if (shownCount >= MaxSeedSummaryLines)
                    break;

                if (shownCount > 0)
                    builder.AppendLine();

                builder.Append(stack.DisplayName);
                shownCount++;
            }

            if (shownCount == 0)
                return "No seeds available";

            int remainingCount = seedStacks.Count - shownCount;

            if (remainingCount > 0)
            {
                builder.AppendLine();
                builder.Append($"+{remainingCount} more...");
            }

            return builder.ToString();
        }

        private static string BuildProductSummary(List<HarvestProductInstance> products)
        {
            if (products == null || products.Count == 0)
                return "No products available";

            StringBuilder builder = new StringBuilder();
            int shownCount = 0;

            foreach (HarvestProductInstance product in products)
            {
                if (product == null)
                    continue;

                if (shownCount >= MaxProductSummaryLines)
                    break;

                if (shownCount > 0)
                    builder.AppendLine();

                builder.Append(FormatProductSummaryLine(product));
                shownCount++;
            }

            if (shownCount == 0)
                return "No products available";

            int remainingCount = products.Count - shownCount;

            if (remainingCount > 0)
            {
                builder.AppendLine();
                builder.Append($"+{remainingCount} more...");
            }

            return builder.ToString();
        }

        private static string FormatProductSummaryLine(HarvestProductInstance product)
        {
            string strainName = string.IsNullOrEmpty(product.strainName) ? "Unknown Product" : product.strainName;
            string grade = string.IsNullOrEmpty(product.gradeLetter) ? "?" : product.gradeLetter;
            string shinyPrefix = product.isShiny ? "Shiny " : "";

            return $"{shinyPrefix}{strainName} | {grade} | {product.grams:0.0}g | ${product.TotalValue}";
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

            PlantStrainData harvestedStrain = plant.strainData;
            HarvestProductInstance product = HarvestProcessor.CreateHarvestProduct(plant);

            if (product == null)
            {
                Debug.LogWarning("Harvest failed: plant could not create product.");
                Refresh();
                return;
            }

            DiscoverHarvestedStrain(harvestedStrain);
            productInventory.AddProduct(product);
            currentNode.RemovePlant();

            Debug.Log($"Harvested hex plant: {product.DisplayName}");
            Refresh();
        }

        private void DiscoverHarvestedStrain(PlantStrainData strain)
        {
            if (strain == null)
                return;

            if (discoveryManager == null)
                discoveryManager = DiscoveryManager.Instance;

            if (discoveryManager == null)
            {
                Debug.LogWarning("HexPanelController: No DiscoveryManager assigned.");
                return;
            }

            discoveryManager.DiscoverStrain(strain);
        }

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

        public void UpgradeShelf()
        {
            if (tableController == null)
            {
                Debug.LogWarning("Cannot upgrade shelf: no TableController assigned.");
                return;
            }

            bool upgraded = tableController.TryUpgradeShelf(economyManager, shelfUpgradeCost);

            if (upgraded)
                Refresh();
        }

        public void UnlockShelfSlot2()
        {
            TryUnlockShelfSlot(UnlockShelf2SlotIndex, shelf2UnlockCost);
        }

        public void UnlockShelfSlot6()
        {
            TryUnlockShelfSlot(UnlockShelf6SlotIndex, shelf6UnlockCost);
        }

        private void TryUnlockShelfSlot(int slotIndex, int unlockCost)
        {
            if (rackController == null)
            {
                Debug.LogWarning($"Cannot unlock shelf slot {slotIndex}: no RackController assigned.");
                return;
            }

            if (economyManager == null)
            {
                Debug.LogWarning($"Cannot unlock shelf slot {slotIndex}: no EconomyManager assigned.");
                return;
            }

            bool unlocked = rackController.TryUnlockShelfSlot(slotIndex, economyManager, unlockCost);

            if (unlocked)
                Refresh();
        }

        private string GetNextSeedText()
        {
            if (seedInventory == null)
                return "Next Seed: -";

            var seeds = seedInventory.GetAllSeeds();

            if (seeds == null || seeds.Count == 0)
                return "Next Seed: None";

            return $"Next Seed: {seeds[0].DisplayName}";
        }
    }
}
