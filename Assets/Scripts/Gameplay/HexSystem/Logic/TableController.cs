using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Vertigro.Logic
{
    public class TableController : MonoBehaviour
    {
        [Header("References")]
        public TableGenerator generator;
        public TowerManager towerManager;
        public HexSelectionController selectionController;

        [Header("Shelf Identity")]
        [SerializeField] private string shelfId = TowerManager.DefaultShelfId;
        
        [Header("Shelf Settings")]
        [FormerlySerializedAs("currentLevel")]
        public int currentShelfLevel = 1;
        public int floorIndex = 0;
        [FormerlySerializedAs("maxRackLevel")]
        public int maxShelfLevel = 4;

        public string ShelfId => TowerManager.NormalizeShelfId(shelfId);
        public bool IsMaxShelfLevelReached => currentShelfLevel >= maxShelfLevel;

        void Start()
        {
            BuildTable();
        }

        public void BuildTable()
        {
            int rows = Mathf.Max(1, (int)Mathf.Pow(2, currentShelfLevel - 1));
            int cols = 6;

            if (currentShelfLevel >= 4)
                cols = 12;

            string activeShelfId = ShelfId;

            Debug.Log($"Building Shelf {activeShelfId} Capacity Level {currentShelfLevel}: {rows}x{cols}");

            if (selectionController != null)
                selectionController.ClearSelectionForRegeneration();

            if (towerManager != null)
                towerManager.ClearShelfGrid(activeShelfId);

            generator.GenerateTable(rows, cols, floorIndex, activeShelfId, this);
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
