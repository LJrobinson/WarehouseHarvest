using System.Collections.Generic;
using UnityEngine;

namespace Vertigro.Logic
{
    public class RackController : MonoBehaviour
    {
        public const int ShelfSlotCount = 6;

        [SerializeField] private List<ShelfSlotRecord> shelfSlots = new List<ShelfSlotRecord>(ShelfSlotCount);

        public IReadOnlyList<ShelfSlotRecord> ShelfSlots => shelfSlots;

        private void Reset()
        {
            EnsureShelfSlots();
        }

        private void Awake()
        {
            EnsureShelfSlots();
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
