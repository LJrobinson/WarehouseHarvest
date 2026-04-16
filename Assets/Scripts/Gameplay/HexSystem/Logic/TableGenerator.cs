using UnityEngine;
using System.Collections.Generic;

namespace Vertigrow.Logic
{
    public class TableGenerator : MonoBehaviour
    {
        public GameObject hexPrefab;
        public TowerManager towerManager;

        [Header("Manual Alignment")]
        // NOTE: If you type a different number in the Unity Inspector, it will OVERRIDE this code during gameplay! NOTE TO GEMINI: GO FUCK YOURSELF.
        public float xSpacing = 1.0f;
        public float ySpacing = 0.6495f;

        private float _hueSelection = 0f;

        // We need a list to remember all the hexes we created on this specific table.
        // This makes it easy to tell them all to update when the turn ends.
        private List<HexNode> _tableNodes = new List<HexNode>();

        public void GenerateTable(int rows, int cols, int floor)
        {
            // Reset hue for a fresh generation
            _hueSelection = Random.value;

            // Clean up old hexes from the screen and our memory list
            foreach (Transform child in transform)
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
            _tableNodes.Clear();

            for (int r = 0; r < rows; r++)
            {
                float xOffset = (r % 2 == 1) ? 0.5f : 0f;

                for (int q = 0; q < cols; q++)
                {
                    float xPos = (q * xSpacing) + xOffset;
                    float yPos = r * ySpacing;
                    float zPos = floor * 3.0f;

                    Vector3 spawnPos = new Vector3(xPos, yPos, zPos);
                    GameObject go = Instantiate(hexPrefab, spawnPos, Quaternion.identity, this.transform);
                    go.name = $"Hex_F{floor}_R{r}_C{q}";

                    // --- ENHANCED COLOR DISTRIBUTION ---
                    MeshRenderer renderer = go.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null)
                    {
                        // Use the Golden Ratio conjugate (0.618...) to jump around the color wheel
                        // This prevents neighboring hexes from looking too similar
                        _hueSelection = (_hueSelection + 0.618033988749895f) % 1;
                        renderer.material.color = Color.HSVToRGB(_hueSelection, 0.8f, 0.9f);
                    }

                    HexNode node = go.GetComponent<HexNode>();

                    // Save this node to our local table list so we can calculate it later
                    if (node != null)
                    {
                        _tableNodes.Add(node);
                    }

                    if (towerManager != null) towerManager.RegisterNode(q, r, floor, node);
                }
            }
        }

        /// <summary>
        /// This is the "Next Day" calculation for this specific table.
        /// It avoids the heavy Update() loop by only running when the turn changes.
        /// </summary>
        public void TableTick()
        {
            Debug.Log("Table Calculation: Processing Next Day...");

            foreach (HexNode node in _tableNodes)
            {
                if (node != null)
                {
                    // Tell the individual hex to process its growth
                    node.ProcessTick();
                }
            }

            // Future Stage: After all plants grow, we can run a second pass 
            // to calculate adjacency bonuses across the whole shelf.
        }

        /// <summary>
        /// Logic for specific gameplay unlocks.
        /// level 1 = 6 hexes, level 2 = 12, etc.
        /// </summary>
        public void UnlockTableLevel(int level)
        {
            // Base logic: rows increase by 2 per level, columns stay at 6
            // Level 1: (1*2)x6 = 12 | Level 8: (8*2)x6 = 96
            int rowCount = level * 2;
            GenerateTable(rowCount, 6, 0);
        }

        [ContextMenu("Simulate Next Day (Table Tick)")]
        public void TestTableTick() => TableTick();

        [ContextMenu("Unlock Level 1 (12 Hex)")]
        public void TestLvl1() => UnlockTableLevel(1);

        [ContextMenu("Unlock Level 4 (48 Hex)")]
        public void TestLvl4() => UnlockTableLevel(4);

        [ContextMenu("Unlock Level 8 (96 Hex)")]
        public void TestLvl8() => UnlockTableLevel(8);

        [ContextMenu("Stress Test: 1k")]
        public void Stress1k() => GenerateTable(100, 10, 0);

        [ContextMenu("Stress Test: 10k")]
        public void Stress10k() => GenerateTable(100, 100, 0);
    }
}