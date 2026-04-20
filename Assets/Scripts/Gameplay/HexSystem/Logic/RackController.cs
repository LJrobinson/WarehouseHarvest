using System.Collections.Generic;
using UnityEngine;

namespace Vertigro.Logic
{
    public enum UtilityStatus
    {
        Healthy,
        Strained,
        Deficit
    }

    public class RackController : MonoBehaviour
    {
        public const int ShelfSlotCount = 6;
        private const float StrainedDemandCapacityRatio = 0.9f;

        [Header("Utility Capacity Source")]
        [SerializeField] private global::Warehouse utilityCapacitySource;

        [Header("Shelf Slots")]
        [SerializeField] private List<ShelfSlotRecord> shelfSlots = new List<ShelfSlotRecord>(ShelfSlotCount);

        public IReadOnlyList<ShelfSlotRecord> ShelfSlots => shelfSlots;

        private void Reset()
        {
            EnsureShelfSlots();
        }

        private void Awake()
        {
            EnsureShelfSlots();
            AssignUtilityStateSourceToActiveShelves();
        }

        private void OnValidate()
        {
            EnsureShelfSlots();
        }

        public ShelfSlotRecord GetShelfSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > ShelfSlotCount)
                return null;

            EnsureShelfSlots();
            return shelfSlots[slotIndex - 1];
        }

        public bool TryUnlockShelfSlot(int slotIndex, EconomyManager economyManager, int unlockCost)
        {
            ShelfSlotRecord slot = GetShelfSlot(slotIndex);

            if (slot == null)
            {
                Debug.LogWarning($"Cannot unlock shelf slot {slotIndex}: index is outside 1-{ShelfSlotCount}.");
                return false;
            }

            if (slot.isUnlocked)
            {
                Debug.Log($"Shelf slot {slotIndex} is already unlocked.");
                return false;
            }

            if (economyManager == null)
            {
                Debug.LogWarning($"Cannot unlock shelf slot {slotIndex}: no EconomyManager assigned.");
                return false;
            }

            if (unlockCost < 0)
            {
                Debug.LogWarning($"Cannot unlock shelf slot {slotIndex}: unlock cost cannot be negative.");
                return false;
            }

            if (!economyManager.SpendMoney(unlockCost))
            {
                Debug.Log($"Cannot unlock shelf slot {slotIndex}: not enough money.");
                return false;
            }

            slot.isUnlocked = true;
            Debug.Log($"Rack shelf slot {slotIndex} unlocked for ${unlockCost}. No shelf instance assigned.");
            return true;
        }

        public bool TryActivateShelfSlot(
            int slotIndex,
            TableController shelfPrefab,
            TowerManager sharedTowerManager,
            HexSelectionController sharedSelectionController,
            out TableController activatedShelf)
        {
            activatedShelf = null;
            ShelfSlotRecord slot = GetShelfSlot(slotIndex);
            string shelfId = GetShelfIdForSlot(slotIndex);

            if (slot == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: index is outside 1-{ShelfSlotCount}.");
                return false;
            }

            if (!slot.isUnlocked)
            {
                Debug.Log($"Cannot activate shelf slot {slotIndex}: slot is locked.");
                return false;
            }

            if (slot.shelf != null)
            {
                Debug.Log($"Cannot activate shelf slot {slotIndex}: slot already has an active shelf.");
                return false;
            }

            if (slot.anchor == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: no shelf anchor assigned.");
                return false;
            }

            if (shelfPrefab == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: no ShelfUnit prefab assigned.");
                return false;
            }

            if (sharedTowerManager == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: no TowerManager assigned.");
                return false;
            }

            if (sharedSelectionController == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: no HexSelectionController assigned.");
                return false;
            }

            TableController shelfInstance = Instantiate(shelfPrefab, slot.anchor);
            shelfInstance.name = $"Shelf_RackSlot_{slotIndex}";
            shelfInstance.transform.localPosition = Vector3.zero;
            shelfInstance.transform.localRotation = Quaternion.identity;
            shelfInstance.transform.localScale = Vector3.one;

            TableGenerator generator = shelfInstance.generator != null
                ? shelfInstance.generator
                : shelfInstance.GetComponentInChildren<TableGenerator>(true);

            if (generator == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: ShelfUnit has no TableGenerator.");
                CleanupFailedShelfActivation(shelfInstance, sharedTowerManager, shelfId);
                return false;
            }

            if (generator.hexPrefab == null)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: ShelfUnit generator has no hex prefab.");
                CleanupFailedShelfActivation(shelfInstance, sharedTowerManager, shelfId);
                return false;
            }

            try
            {
                shelfInstance.InitializeShelf(shelfId, generator, sharedTowerManager, sharedSelectionController, true);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Failed to activate shelf slot {slotIndex}: {exception.Message}");
                CleanupFailedShelfActivation(shelfInstance, sharedTowerManager, shelfId);
                return false;
            }

            if (generator.TableNodes.Count == 0)
            {
                Debug.LogWarning($"Cannot activate shelf slot {slotIndex}: ShelfUnit built no hex nodes.");
                CleanupFailedShelfActivation(shelfInstance, sharedTowerManager, shelfId);
                return false;
            }

            slot.shelf = shelfInstance;
            shelfInstance.SetUtilityStateSource(this);
            activatedShelf = shelfInstance;

            Debug.Log($"Rack shelf slot {slotIndex} activated as {shelfId}.");
            return true;
        }

        public void TickActiveShelves()
        {
            EnsureShelfSlots();

            int tickedCount = 0;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                TableGenerator generator = ResolveShelfGenerator(slot.shelf);

                if (generator == null)
                {
                    Debug.LogWarning($"Cannot tick shelf slot {slot.slotIndex}: active shelf has no TableGenerator.");
                    continue;
                }

                generator.TableTick();
                tickedCount++;
            }

            Debug.Log($"Rack processed Next Day for {tickedCount} active shelf(s).");
        }

        public int GetUnlockedShelfCount()
        {
            if (shelfSlots == null)
                return 0;

            int count = 0;

            foreach (ShelfSlotRecord slot in shelfSlots)
            {
                if (slot != null && slot.isUnlocked)
                    count++;
            }

            return count;
        }

        public int GetActiveShelfCount()
        {
            int count = 0;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
                count++;

            return count;
        }

        public int GetTotalGrowableHexCount()
        {
            int count = 0;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                TableGenerator generator = ResolveShelfGenerator(slot.shelf);

                if (generator == null || generator.TableNodes == null)
                    continue;

                foreach (HexNode node in generator.TableNodes)
                {
                    if (node != null)
                        count++;
                }
            }

            return count;
        }

        public int GetTotalPlantedHexCount()
        {
            int count = 0;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                TableGenerator generator = ResolveShelfGenerator(slot.shelf);

                if (generator == null || generator.TableNodes == null)
                    continue;

                foreach (HexNode node in generator.TableNodes)
                {
                    if (node != null && node.currentPlant != null)
                        count++;
                }
            }

            return count;
        }

        public int GetTotalEmptyHexCount()
        {
            return GetTotalGrowableHexCount() - GetTotalPlantedHexCount();
        }

        public float GetTotalPowerDemand()
        {
            return GetBasePowerDemand() + GetPlantedLoadPowerDemand();
        }

        public float GetTotalWaterDemand()
        {
            return GetBaseWaterDemand() + GetPlantedLoadWaterDemand();
        }

        public float GetTotalDataDemand()
        {
            return GetBaseDataDemand() + GetPlantedLoadDataDemand();
        }

        public float GetBasePowerDemand()
        {
            float total = 0f;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                if (slot.shelf != null)
                    total += slot.shelf.BasePowerDemand;
            }

            return total;
        }

        public float GetBaseWaterDemand()
        {
            float total = 0f;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                if (slot.shelf != null)
                    total += slot.shelf.BaseWaterDemand;
            }

            return total;
        }

        public float GetBaseDataDemand()
        {
            float total = 0f;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                if (slot.shelf != null)
                    total += slot.shelf.BaseDataDemand;
            }

            return total;
        }

        public float GetPlantedLoadPowerDemand()
        {
            float total = 0f;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                if (slot.shelf != null)
                    total += slot.shelf.PlantedLoadPowerDemand;
            }

            return total;
        }

        public float GetPlantedLoadWaterDemand()
        {
            float total = 0f;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                if (slot.shelf != null)
                    total += slot.shelf.PlantedLoadWaterDemand;
            }

            return total;
        }

        public float GetPlantedLoadDataDemand()
        {
            float total = 0f;

            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
            {
                if (slot.shelf != null)
                    total += slot.shelf.PlantedLoadDataDemand;
            }

            return total;
        }

        public float GetTotalPowerCapacity()
        {
            return utilityCapacitySource != null ? Mathf.Max(0f, utilityCapacitySource.maxPower) : 0f;
        }

        public float GetTotalWaterCapacity()
        {
            return utilityCapacitySource != null ? Mathf.Max(0f, utilityCapacitySource.maxWater) : 0f;
        }

        public float GetTotalDataCapacity()
        {
            return utilityCapacitySource != null ? Mathf.Max(0f, utilityCapacitySource.maxData) : 0f;
        }

        public float GetPowerSurplus()
        {
            return GetTotalPowerCapacity() - GetTotalPowerDemand();
        }

        public float GetWaterSurplus()
        {
            return GetTotalWaterCapacity() - GetTotalWaterDemand();
        }

        public float GetDataSurplus()
        {
            return GetTotalDataCapacity() - GetTotalDataDemand();
        }

        public bool HasPowerDeficit()
        {
            return GetPowerSurplus() < 0f;
        }

        public bool HasWaterDeficit()
        {
            return GetWaterSurplus() < 0f;
        }

        public bool HasDataDeficit()
        {
            return GetDataSurplus() < 0f;
        }

        public UtilityStatus GetPowerStatus()
        {
            return GetUtilityStatus(GetTotalPowerDemand(), GetTotalPowerCapacity(), GetPowerSurplus());
        }

        public UtilityStatus GetWaterStatus()
        {
            return GetUtilityStatus(GetTotalWaterDemand(), GetTotalWaterCapacity(), GetWaterSurplus());
        }

        public UtilityStatus GetDataStatus()
        {
            return GetUtilityStatus(GetTotalDataDemand(), GetTotalDataCapacity(), GetDataSurplus());
        }

        public static string GetShelfIdForSlot(int slotIndex)
        {
            return $"RackSlot_{slotIndex}";
        }

        private void EnsureShelfSlots()
        {
            if (shelfSlots == null)
                shelfSlots = new List<ShelfSlotRecord>(ShelfSlotCount);

            while (shelfSlots.Count < ShelfSlotCount)
                shelfSlots.Add(new ShelfSlotRecord(shelfSlots.Count + 1));

            if (shelfSlots.Count > ShelfSlotCount)
                shelfSlots.RemoveRange(ShelfSlotCount, shelfSlots.Count - ShelfSlotCount);

            for (int i = 0; i < shelfSlots.Count; i++)
            {
                if (shelfSlots[i] == null)
                    shelfSlots[i] = new ShelfSlotRecord(i + 1);

                shelfSlots[i].slotIndex = i + 1;
            }
        }

        private void AssignUtilityStateSourceToActiveShelves()
        {
            foreach (ShelfSlotRecord slot in GetUniqueActiveShelfSlots())
                slot.shelf.SetUtilityStateSource(this);
        }

        private IEnumerable<ShelfSlotRecord> GetUniqueActiveShelfSlots()
        {
            if (shelfSlots == null)
                yield break;

            HashSet<TableController> activeShelves = new HashSet<TableController>();

            foreach (ShelfSlotRecord slot in shelfSlots)
            {
                if (slot == null || !slot.isUnlocked || slot.shelf == null)
                    continue;

                if (!activeShelves.Add(slot.shelf))
                    continue;

                yield return slot;
            }
        }

        private static TableGenerator ResolveShelfGenerator(TableController shelf)
        {
            if (shelf == null)
                return null;

            return shelf.generator != null
                ? shelf.generator
                : shelf.GetComponentInChildren<TableGenerator>(true);
        }

        private static UtilityStatus GetUtilityStatus(float demand, float capacity, float surplus)
        {
            if (surplus < 0f)
                return UtilityStatus.Deficit;

            if (capacity <= 0f)
                return UtilityStatus.Healthy;

            float demandCapacityRatio = demand / capacity;
            return demandCapacityRatio >= StrainedDemandCapacityRatio
                ? UtilityStatus.Strained
                : UtilityStatus.Healthy;
        }

        private static void CleanupFailedShelfActivation(TableController shelfInstance, TowerManager towerManager, string shelfId)
        {
            if (towerManager != null)
                towerManager.ClearShelfGrid(shelfId);

            if (shelfInstance == null)
                return;

            if (Application.isPlaying)
                Destroy(shelfInstance.gameObject);
            else
                DestroyImmediate(shelfInstance.gameObject);
        }
    }

    [System.Serializable]
    public class ShelfSlotRecord
    {
        public int slotIndex = 1;
        public bool isUnlocked;
        public TableController shelf;
        public Transform anchor;

        public ShelfSlotRecord()
        {
        }

        public ShelfSlotRecord(int slotIndex)
        {
            this.slotIndex = slotIndex;
        }
    }
}
