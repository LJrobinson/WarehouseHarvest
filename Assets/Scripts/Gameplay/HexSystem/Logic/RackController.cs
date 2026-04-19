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
