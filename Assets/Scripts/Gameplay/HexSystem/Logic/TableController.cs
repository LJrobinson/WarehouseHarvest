using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Vertigro.Logic
{
    public class TableController : MonoBehaviour
    {
        private const float NormalUtilityGrowthMultiplier = 1f;
        private const float StrainedUtilityGrowthMultiplier = 0.75f;
        private const float DeficitUtilityGrowthMultiplier = 0.5f;

        [Header("References")]
        public TableGenerator generator;
        public TowerManager towerManager;
        public HexSelectionController selectionController;
        [SerializeField] private RackController utilityStateSourceRack;

        [Header("Shelf Identity")]
        [SerializeField] private string shelfId = TowerManager.DefaultShelfId;
        [SerializeField] private bool autoBuildOnStart = true;
        
        [Header("Shelf Settings")]
        [FormerlySerializedAs("currentLevel")]
        public int currentShelfLevel = 1;
        public int floorIndex = 0;
        [FormerlySerializedAs("maxRackLevel")]
        public int maxShelfLevel = 4;

        [Header("Shelf Utility Demand")]
        [SerializeField] private float powerDemand = 50f;
        [SerializeField] private float waterDemand = 25f;
        [SerializeField] private float dataDemand = 10f;

        [Header("Shelf Utility Demand Scaling")]
        [SerializeField] private float powerDemandPerPlantedHex = 1f;
        [SerializeField] private float waterDemandPerPlantedHex = 0.5f;
        [SerializeField] private float dataDemandPerPlantedHex = 0.1f;

        public string ShelfId => TowerManager.NormalizeShelfId(shelfId);
        public bool IsMaxShelfLevelReached => currentShelfLevel >= maxShelfLevel;
        public int GrowableHexCount => CountGrowableHexes();
        public int PlantedHexCount => CountPlantedHexes();
        public float BasePowerDemand => Mathf.Max(0f, powerDemand);
        public float BaseWaterDemand => Mathf.Max(0f, waterDemand);
        public float BaseDataDemand => Mathf.Max(0f, dataDemand);
        public float PlantedLoadPowerDemand => GetPlantedLoadDemand(powerDemandPerPlantedHex);
        public float PlantedLoadWaterDemand => GetPlantedLoadDemand(waterDemandPerPlantedHex);
        public float PlantedLoadDataDemand => GetPlantedLoadDemand(dataDemandPerPlantedHex);
        public float PowerDemand => BasePowerDemand + PlantedLoadPowerDemand;
        public float WaterDemand => BaseWaterDemand + PlantedLoadWaterDemand;
        public float DataDemand => BaseDataDemand + PlantedLoadDataDemand;
        public bool HasSufficientPower => PowerUtilityStatus != UtilityStatus.Deficit;
        public bool HasSufficientWater => WaterUtilityStatus != UtilityStatus.Deficit;
        public bool HasSufficientData => DataUtilityStatus != UtilityStatus.Deficit;
        public UtilityStatus PowerUtilityStatus => utilityStateSourceRack != null ? utilityStateSourceRack.GetPowerStatus() : UtilityStatus.Healthy;
        public UtilityStatus WaterUtilityStatus => utilityStateSourceRack != null ? utilityStateSourceRack.GetWaterStatus() : UtilityStatus.Healthy;
        public UtilityStatus DataUtilityStatus => utilityStateSourceRack != null ? utilityStateSourceRack.GetDataStatus() : UtilityStatus.Healthy;
        public float UtilityGrowthMultiplier
        {
            get
            {
                UtilityStatus powerStatus = PowerUtilityStatus;
                UtilityStatus waterStatus = WaterUtilityStatus;
                UtilityStatus dataStatus = DataUtilityStatus;

                if (powerStatus == UtilityStatus.Deficit ||
                    waterStatus == UtilityStatus.Deficit ||
                    dataStatus == UtilityStatus.Deficit)
                    return DeficitUtilityGrowthMultiplier;

                if (powerStatus == UtilityStatus.Strained ||
                    waterStatus == UtilityStatus.Strained ||
                    dataStatus == UtilityStatus.Strained)
                    return StrainedUtilityGrowthMultiplier;

                return NormalUtilityGrowthMultiplier;
            }
        }

        private bool wasInitialized;

        void Start()
        {
            ApplySharedReferencesToGenerator();

            if (!autoBuildOnStart || wasInitialized)
                return;

            BuildTable();
        }

        public void InitializeShelf(
            string newShelfId,
            TableGenerator shelfGenerator,
            TowerManager sharedTowerManager,
            HexSelectionController sharedSelectionController,
            bool buildAfterInitialize = true)
        {
            shelfId = TowerManager.NormalizeShelfId(newShelfId);

            if (shelfGenerator != null)
                generator = shelfGenerator;

            if (sharedTowerManager != null)
                towerManager = sharedTowerManager;

            if (sharedSelectionController != null)
                selectionController = sharedSelectionController;

            ApplySharedReferencesToGenerator();
            wasInitialized = true;

            if (buildAfterInitialize)
                BuildTable();
        }

        public void InitializeShelf(string newShelfId, bool buildAfterInitialize = true)
        {
            InitializeShelf(newShelfId, generator, towerManager, selectionController, buildAfterInitialize);
        }

        public void SetUtilityStateSource(RackController rackController)
        {
            if (rackController != null)
                utilityStateSourceRack = rackController;
        }

        private float GetPlantedLoadDemand(float plantedHexDemand)
        {
            return PlantedHexCount * Mathf.Max(0f, plantedHexDemand);
        }

        private int CountGrowableHexes()
        {
            if (generator == null || generator.TableNodes == null)
                return 0;

            int count = 0;

            foreach (HexNode node in generator.TableNodes)
            {
                if (node != null)
                    count++;
            }

            return count;
        }

        private int CountPlantedHexes()
        {
            if (generator == null || generator.TableNodes == null)
                return 0;

            int count = 0;

            foreach (HexNode node in generator.TableNodes)
            {
                if (node != null && node.currentPlant != null)
                    count++;
            }

            return count;
        }

        public void BuildTable()
        {
            string activeShelfId = ShelfId;

            ApplySharedReferencesToGenerator();

            if (generator == null)
            {
                Debug.LogWarning($"Cannot build shelf {activeShelfId}: no TableGenerator assigned.");
                return;
            }

            int rows = Mathf.Max(1, (int)Mathf.Pow(2, currentShelfLevel - 1));
            int cols = 6;

            if (currentShelfLevel >= 4)
                cols = 12;

            Debug.Log($"Building Shelf {activeShelfId} Capacity Level {currentShelfLevel}: {rows}x{cols}");

            if (selectionController != null)
                selectionController.ClearSelectionForRegeneration();

            if (towerManager != null)
                towerManager.ClearShelfGrid(activeShelfId);

            generator.GenerateTable(rows, cols, floorIndex, activeShelfId, this);
        }

        private void ApplySharedReferencesToGenerator()
        {
            if (generator != null && towerManager != null)
                generator.towerManager = towerManager;
        }

        public bool IsShelfEmpty()
        {
            if (towerManager == null)
            {
                Debug.LogWarning("Cannot check shelf: no TowerManager assigned.");
                return false;
            }

            foreach (HexNode node in towerManager.GetShelfNodes(ShelfId))
            {
                if (node != null && !node.IsEmpty)
                    return false;
            }

            return true;
        }

        public bool TryUpgradeShelf(EconomyManager economyManager, int upgradeCost)
        {
            if (generator == null)
            {
                Debug.LogWarning("Cannot upgrade shelf: no TableGenerator assigned.");
                return false;
            }

            if (towerManager == null)
            {
                Debug.LogWarning("Cannot upgrade shelf: no TowerManager assigned.");
                return false;
            }

            if (IsMaxShelfLevelReached)
            {
                Debug.Log("Cannot upgrade shelf: shelf is already at max capacity.");
                return false;
            }

            if (!IsShelfEmpty())
            {
                Debug.Log("Cannot upgrade shelf: shelf is not empty.");
                return false;
            }

            if (economyManager == null)
            {
                Debug.LogWarning("Cannot upgrade shelf: no EconomyManager assigned.");
                return false;
            }

            if (!economyManager.SpendMoney(upgradeCost))
            {
                Debug.Log("Cannot upgrade shelf: not enough money.");
                return false;
            }

            currentShelfLevel++;
            BuildTable();

            Debug.Log($"Shelf capacity upgraded to level {currentShelfLevel}.");
            return true;
        }
    }
}
