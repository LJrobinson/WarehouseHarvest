using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
        [SerializeField] private Button upgradePowerCapacityButton;
        [SerializeField] private Button upgradeWaterCapacityButton;
        [SerializeField] private Button upgradeDataCapacityButton;

        [Header("Gameplay References")]
        [SerializeField] private DiscoveryManager discoveryManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private GameObject plantPrefab;
        [SerializeField] private ProductInventory productInventory;        
        [SerializeField] private SeedInventory seedInventory;
        [SerializeField] private HexSelectionController selectionController;
        [SerializeField] private TableController tableController;
        [SerializeField] private RackController rackController;
        [SerializeField] private TableController shelfUnitPrefab;

        [Header("Shelf Capacity Upgrade")]
        [FormerlySerializedAs("rackUpgradeCost")]
        [SerializeField] private int shelfUpgradeCost = 500;

        [Header("Rack Shelf Unlock")]
        [SerializeField] private int shelf2UnlockCost = 250;
        [SerializeField] private int shelf6UnlockCost = 250;

        [Header("Utility Capacity Upgrade")]
        [SerializeField] private int powerCapacityUpgradeCost = 500;
        [SerializeField] private float powerCapacityUpgradeAmount = 50f;
        [SerializeField] private int waterCapacityUpgradeCost = 500;
        [SerializeField] private float waterCapacityUpgradeAmount = 50f;
        [SerializeField] private int dataCapacityUpgradeCost = 500;
        [SerializeField] private float dataCapacityUpgradeAmount = 50f;

        [Header("Dev Utility Capacity Test")]
        [SerializeField] private bool enableDevUtilityCapacityHotkeys = true;
        [SerializeField] private float devUtilityCapacityIncrease = 50f;
        
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

            HandleDevUtilityCapacityHotkeys();
            Refresh();
        }

        public void Refresh()
        {
            if (selectionController == null)
            {
                SetHarvestButtonInteractable(false);
                SetUpgradeButtonState();
                SetUnlockShelfButtonStates();
                SetUtilityCapacityUpgradeButtonStates();
                RefreshRackSlotSummary();
                SetText(stateText, "ERROR: No Selection Controller");
                return;
            }

            if (modeText != null)
                modeText.text = $"Mode: {selectionController.CurrentPlacementMode}\n{BuildUtilityWarningText(rackController)}";

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
            SetUtilityCapacityUpgradeButtonStates();
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
            AppendSelectedShelfUtilityText(currentNode);
            
            RefreshSellAllButton();
            RefreshModeButtons();
        }

        private void AppendSelectedShelfUtilityText(HexNode node)
        {
            if (node == null || node.OwningShelf == null || stateText == null)
                return;

            TableController shelf = node.OwningShelf;
            stateText.text +=
                $"\nShelf Utilities: P {FormatShelfUtilitySignal(shelf.HasSufficientPower, shelf.PowerUtilityStatus)}" +
                $" | W {FormatShelfUtilitySignal(shelf.HasSufficientWater, shelf.WaterUtilityStatus)}" +
                $" | D {FormatShelfUtilitySignal(shelf.HasSufficientData, shelf.DataUtilityStatus)}" +
                $" | Live Growth x{shelf.UtilityGrowthMultiplier:0.##}" +
                $"\nShelf Load: {shelf.PlantedHexCount}/{shelf.GrowableHexCount} planted" +
                $" | Demand P {shelf.PowerDemand:0.#} W {shelf.WaterDemand:0.#} D {shelf.DataDemand:0.#}";
        }

        private static string FormatShelfUtilitySignal(bool hasSufficientUtility, UtilityStatus status)
        {
            return hasSufficientUtility ? status.ToString() : "Insufficient";
        }

        private static string BuildUtilityWarningText(RackController rack)
        {
            if (rack == null)
                return "Utilities: Unknown";

            UtilityStatus status = rack.GetOverallUtilityStatus();
            UtilityType bottleneck = rack.GetMostConstrainedUtility();

            if (status == UtilityStatus.Healthy || bottleneck == UtilityType.None)
                return $"Utilities: {status}";

            return $"Utilities: {status} ({bottleneck})";
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

            AppendRackDebugSummary(builder, rack);
            return builder.ToString();
        }

        private static void AppendRackDebugSummary(StringBuilder builder, RackController rack)
        {
            builder.AppendLine();
            builder.AppendLine();
            builder.Append("Rack Totals");
            builder.AppendLine();
            builder.Append($"Unlocked Shelves: {rack.GetUnlockedShelfCount()}");
            builder.AppendLine();
            builder.Append($"Active Shelves: {rack.GetActiveShelfCount()}");
            builder.AppendLine();
            builder.Append($"Total Growable Hexes: {rack.GetTotalGrowableHexCount()}");
            builder.AppendLine();
            builder.Append($"Total Planted Hexes: {rack.GetTotalPlantedHexCount()}");
            builder.AppendLine();
            builder.Append($"Total Empty Hexes: {rack.GetTotalEmptyHexCount()}");
            builder.AppendLine();
            builder.Append($"Power: {FormatUtilityComparison(rack.GetTotalPowerDemand(), rack.GetTotalPowerCapacity(), rack.GetPowerSurplus(), rack.GetPowerStatus(), rack.GetBasePowerDemand(), rack.GetPlantedLoadPowerDemand())}");
            builder.AppendLine();
            builder.Append($"Water: {FormatUtilityComparison(rack.GetTotalWaterDemand(), rack.GetTotalWaterCapacity(), rack.GetWaterSurplus(), rack.GetWaterStatus(), rack.GetBaseWaterDemand(), rack.GetPlantedLoadWaterDemand())}");
            builder.AppendLine();
            builder.Append($"Data: {FormatUtilityComparison(rack.GetTotalDataDemand(), rack.GetTotalDataCapacity(), rack.GetDataSurplus(), rack.GetDataStatus(), rack.GetBaseDataDemand(), rack.GetPlantedLoadDataDemand())}");
            builder.AppendLine();
            builder.Append($"Utility Pressure: {rack.GetMostConstrainedUtility()} @ {FormatUtilityUtilization(rack.GetHighestUtilityUtilization())} ({rack.GetOverallUtilityStatus()})");
        }

        private static string FormatUtilityComparison(
            float demand,
            float capacity,
            float surplus,
            UtilityStatus status,
            float baseDemand,
            float plantedLoadDemand)
        {
            string sign = surplus > 0f ? "+" : "";
            return $"{demand:0.#}/{capacity:0.#} ({sign}{surplus:0.#}, {status}; {baseDemand:0.#} base + {plantedLoadDemand:0.#} planted)";
        }

        private static string FormatUtilityUtilization(float utilization)
        {
            return float.IsPositiveInfinity(utilization) ? "Inf" : utilization.ToString("0.##");
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

            TableController targetShelf = ResolveTargetShelf();
            bool hasTargetShelf = targetShelf != null;
            bool isMaxed = hasTargetShelf && targetShelf.IsMaxShelfLevelReached;
            upgradeShelfButton.interactable = hasTargetShelf && !isMaxed;

            TMP_Text upgradeLabel = upgradeShelfButton.GetComponentInChildren<TMP_Text>();
            if (upgradeLabel != null)
            {
                if (!hasTargetShelf)
                    upgradeLabel.text = "No Shelf";
                else
                    upgradeLabel.text = isMaxed
                        ? "Shelf Maxed"
                        : $"Upgrade Shelf (${shelfUpgradeCost})";
            }
        }

        private TableController ResolveTargetShelf()
        {
            HexNode selectedNode = selectionController != null ? selectionController.SelectedNode : null;

            if (selectedNode != null && selectedNode.OwningShelf != null)
                return selectedNode.OwningShelf;

            return tableController;
        }

        private void SetUnlockShelfButtonStates()
        {
            SetUnlockShelfButtonState(unlockShelf2Button, UnlockShelf2SlotIndex, shelf2UnlockCost);
            SetShelf6ButtonState();
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

        private void SetShelf6ButtonState()
        {
            if (unlockShelf6Button == null)
                return;

            ShelfSlotRecord slot = rackController != null ? rackController.GetShelfSlot(UnlockShelf6SlotIndex) : null;
            bool canAfford = economyManager != null && economyManager.Money >= shelf6UnlockCost;
            bool hasValidCost = shelf6UnlockCost >= 0;
            bool canUnlock = rackController != null && slot != null && !slot.isUnlocked && canAfford && hasValidCost;
            bool canActivate = CanActivateShelfSlot(slot);

            unlockShelf6Button.interactable = canUnlock || canActivate;

            TMP_Text unlockLabel = unlockShelf6Button.GetComponentInChildren<TMP_Text>();
            if (unlockLabel == null)
                return;

            if (slot == null)
                unlockLabel.text = "Shelf 6 Unavailable";
            else if (!slot.isUnlocked)
                unlockLabel.text = $"Unlock Shelf 6 (${shelf6UnlockCost})";
            else if (slot.shelf != null)
                unlockLabel.text = "Shelf 6 Active";
            else
                unlockLabel.text = "Activate Shelf 6";
        }

        private void SetUtilityCapacityUpgradeButtonStates()
        {
            SetUtilityCapacityUpgradeButtonState(upgradePowerCapacityButton, "Power", powerCapacityUpgradeCost, powerCapacityUpgradeAmount);
            SetUtilityCapacityUpgradeButtonState(upgradeWaterCapacityButton, "Water", waterCapacityUpgradeCost, waterCapacityUpgradeAmount);
            SetUtilityCapacityUpgradeButtonState(upgradeDataCapacityButton, "Data", dataCapacityUpgradeCost, dataCapacityUpgradeAmount);
        }

        private void SetUtilityCapacityUpgradeButtonState(Button upgradeButton, string utilityName, int upgradeCost, float upgradeAmount)
        {
            if (upgradeButton == null)
                return;

            bool hasWarehouse = ResolveUtilityCapacitySource() != null;
            bool hasEconomy = economyManager != null;
            bool hasValidCost = upgradeCost >= 0;
            bool hasValidAmount = IsPositiveFinite(upgradeAmount);
            bool canAfford = hasEconomy && economyManager.Money >= upgradeCost;

            upgradeButton.interactable = hasWarehouse
                                         && hasEconomy
                                         && hasValidCost
                                         && hasValidAmount
                                         && canAfford;

            TMP_Text upgradeLabel = upgradeButton.GetComponentInChildren<TMP_Text>();
            if (upgradeLabel == null)
                return;

            if (!hasWarehouse || !hasValidCost || !hasValidAmount)
                upgradeLabel.text = $"{utilityName} Upgrade Unavailable";
            else
                upgradeLabel.text = $"Upgrade {utilityName} +{upgradeAmount:0.#} (${upgradeCost})";
        }

        private bool CanActivateShelfSlot(ShelfSlotRecord slot)
        {
            return rackController != null
                   && slot != null
                   && slot.isUnlocked
                   && slot.shelf == null
                   && slot.anchor != null
                   && shelfUnitPrefab != null
                   && ResolveSharedTowerManager() != null
                   && selectionController != null;
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

        public void DevAddPowerCapacity()
        {
            if (TryAddPowerCapacity(devUtilityCapacityIncrease))
                Refresh();
        }

        public void DevAddWaterCapacity()
        {
            if (TryAddWaterCapacity(devUtilityCapacityIncrease))
                Refresh();
        }

        public void DevAddDataCapacity()
        {
            if (TryAddDataCapacity(devUtilityCapacityIncrease))
                Refresh();
        }

        public void BuyPowerCapacityUpgrade()
        {
            BuyUtilityCapacityUpgrade(UtilityCapacityUpgradeTarget.Power, "power", powerCapacityUpgradeCost, powerCapacityUpgradeAmount);
        }

        public void BuyWaterCapacityUpgrade()
        {
            BuyUtilityCapacityUpgrade(UtilityCapacityUpgradeTarget.Water, "water", waterCapacityUpgradeCost, waterCapacityUpgradeAmount);
        }

        public void BuyDataCapacityUpgrade()
        {
            BuyUtilityCapacityUpgrade(UtilityCapacityUpgradeTarget.Data, "data", dataCapacityUpgradeCost, dataCapacityUpgradeAmount);
        }

        private void BuyUtilityCapacityUpgrade(UtilityCapacityUpgradeTarget target, string utilityName, int upgradeCost, float upgradeAmount)
        {
            global::Warehouse warehouse = ResolveUtilityCapacitySource();

            if (warehouse == null)
            {
                Debug.LogWarning($"Cannot upgrade {utilityName} capacity: no Warehouse capacity source assigned.");
                return;
            }

            if (economyManager == null)
            {
                Debug.LogWarning($"Cannot upgrade {utilityName} capacity: no EconomyManager assigned.");
                return;
            }

            if (upgradeCost < 0 || !IsPositiveFinite(upgradeAmount))
            {
                Debug.LogWarning($"Cannot upgrade {utilityName} capacity: invalid cost or capacity amount.");
                return;
            }

            if (!economyManager.SpendMoney(upgradeCost))
            {
                Refresh();
                return;
            }

            AddUtilityCapacity(warehouse, target, upgradeAmount);
            Refresh();
        }

        private static void AddUtilityCapacity(global::Warehouse warehouse, UtilityCapacityUpgradeTarget target, float upgradeAmount)
        {
            switch (target)
            {
                case UtilityCapacityUpgradeTarget.Power:
                    warehouse.AddPowerCapacity(upgradeAmount);
                    break;
                case UtilityCapacityUpgradeTarget.Water:
                    warehouse.AddWaterCapacity(upgradeAmount);
                    break;
                case UtilityCapacityUpgradeTarget.Data:
                    warehouse.AddDataCapacity(upgradeAmount);
                    break;
            }
        }

        public void UpgradeShelf()
        {
            TableController targetShelf = ResolveTargetShelf();

            if (targetShelf == null)
            {
                Debug.LogWarning("Cannot upgrade shelf: no target TableController resolved.");
                return;
            }

            bool upgraded = targetShelf.TryUpgradeShelf(economyManager, shelfUpgradeCost);

            if (upgraded)
                Refresh();
        }

        public void UnlockShelfSlot2()
        {
            TryUnlockShelfSlot(UnlockShelf2SlotIndex, shelf2UnlockCost);
        }

        public void UnlockShelfSlot6()
        {
            HandleShelfSlot6Action();
        }

        public void HandleShelfSlot6Action()
        {
            ShelfSlotRecord slot = rackController != null ? rackController.GetShelfSlot(UnlockShelf6SlotIndex) : null;

            if (slot == null || !slot.isUnlocked)
            {
                TryUnlockShelfSlot(UnlockShelf6SlotIndex, shelf6UnlockCost);
                return;
            }

            if (slot.shelf != null)
            {
                Refresh();
                return;
            }

            TryActivateShelfSlot6();
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

        private void TryActivateShelfSlot6()
        {
            if (rackController == null)
            {
                Debug.LogWarning("Cannot activate shelf slot 6: no RackController assigned.");
                return;
            }

            TowerManager sharedTowerManager = ResolveSharedTowerManager();

            bool activated = rackController.TryActivateShelfSlot(
                UnlockShelf6SlotIndex,
                shelfUnitPrefab,
                sharedTowerManager,
                selectionController,
                out _);

            if (activated)
                Refresh();
        }

        private TowerManager ResolveSharedTowerManager()
        {
            if (tableController != null && tableController.towerManager != null)
                return tableController.towerManager;

            TableController targetShelf = ResolveTargetShelf();
            return targetShelf != null ? targetShelf.towerManager : null;
        }

        private void HandleDevUtilityCapacityHotkeys()
        {
            if (!enableDevUtilityCapacityHotkeys)
                return;

            if (!gameObject.activeInHierarchy)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.digit7Key.wasPressedThisFrame)
                DevAddPowerCapacity();

            if (keyboard.digit8Key.wasPressedThisFrame)
                DevAddWaterCapacity();

            if (keyboard.digit9Key.wasPressedThisFrame)
                DevAddDataCapacity();
        }

        private bool TryAddPowerCapacity(float amount)
        {
            global::Warehouse warehouse = ResolveUtilityCapacitySource();

            if (warehouse == null)
                return false;

            warehouse.AddPowerCapacity(amount);
            return true;
        }

        private bool TryAddWaterCapacity(float amount)
        {
            global::Warehouse warehouse = ResolveUtilityCapacitySource();

            if (warehouse == null)
                return false;

            warehouse.AddWaterCapacity(amount);
            return true;
        }

        private bool TryAddDataCapacity(float amount)
        {
            global::Warehouse warehouse = ResolveUtilityCapacitySource();

            if (warehouse == null)
                return false;

            warehouse.AddDataCapacity(amount);
            return true;
        }

        private global::Warehouse ResolveUtilityCapacitySource()
        {
            return rackController != null ? rackController.UtilityCapacitySource : null;
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private enum UtilityCapacityUpgradeTarget
        {
            Power,
            Water,
            Data
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
