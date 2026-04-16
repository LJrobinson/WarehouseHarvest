using UnityEngine;
using System.Collections.Generic;

namespace Vertigro.Logic
{
    public class TableController : MonoBehaviour
    {
        [Header("References")]
        public TableGenerator generator;
        public TowerManager towerManager;

        [Header("Table Settings")]
        public int currentLevel = 1;
        public int floorIndex = 0;

        void Start()
        {
            BuildTable();
        }

        public void BuildTable()
        {
            // Define your scaling logic here
            int rows = Mathf.Max(1, (int)Mathf.Pow(2, currentLevel - 1));
            int cols = 6;

            // If it's a high level, maybe we widen the table too
            if (currentLevel >= 4) cols = 12;

            Debug.Log($"Building Table Level {currentLevel}: {rows}x{cols}");
            generator.GenerateTable(rows, cols, floorIndex);
        }
    }
}