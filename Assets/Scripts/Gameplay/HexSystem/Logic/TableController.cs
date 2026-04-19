using UnityEngine;
using System.Collections.Generic;

namespace Vertigro.Logic
{
    public class TableController : MonoBehaviour
    {
        [Header("References")]
        public TableGenerator generator;
        public TowerManager towerManager;
        public HexSelectionController selectionController;
        
        [Header("Table Settings")]
        public int currentLevel = 1;
        public int floorIndex = 0;
        public int maxRackLevel = 4;

        public bool IsMaxRackLevelReached => currentLevel >= maxRackLevel;

        void Start()
        {
            BuildTable();
        }

        public void BuildTable()
        {
            int rows = Mathf.Max(1, (int)Mathf.Pow(2, currentLevel - 1));
            int cols = 6;

            if (currentLevel >= 4)
                cols = 12;

            Debug.Log($"Building Table Level {currentLevel}: {rows}x{cols}");

            if (selectionController != null)
                selectionController.ClearSelectionForRegeneration();

            if (towerManager != null)
                towerManager.ClearGrid();

            generator.GenerateTable(rows, cols, floorIndex);
        }

        public bool IsRackEmpty()
        {
            if (towerManager == null)
            {
                Debug.LogWarning("Cannot check rack: no TowerManager assigned.");
                return false;
            }

            foreach (var entry in towerManager.Grid)
            {
                HexNode node = entry.Value;

                if (node != null && !node.IsEmpty)
                    return false;
            }

            return true;
        }

        public bool TryUpgradeRack(EconomyManager economyManager, int upgradeCost)
        {
            if (generator == null)
            {
                Debug.LogWarning("Cannot upgrade rack: no TableGenerator assigned.");
                return false;
            }

            if (towerManager == null)
            {
                Debug.LogWarning("Cannot upgrade rack: no TowerManager assigned.");
                return false;
            }

            if (IsMaxRackLevelReached)
            {
                Debug.Log("Cannot upgrade rack: rack is already at max level.");
                return false;
            }

            if (!IsRackEmpty())
            {
                Debug.Log("Cannot upgrade rack: rack is not empty.");
                return false;
            }

            if (economyManager == null)
            {
                Debug.LogWarning("Cannot upgrade rack: no EconomyManager assigned.");
                return false;
            }

            if (!economyManager.SpendMoney(upgradeCost))
            {
                Debug.Log("Cannot upgrade rack: not enough money.");
                return false;
            }

            currentLevel++;
            BuildTable();

            Debug.Log($"Rack upgraded to level {currentLevel}.");
            return true;
        }
    }
}