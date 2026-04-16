using UnityEngine;
using System.Collections.Generic;

namespace Vertigro.Logic
{
    public class TableGenerator : MonoBehaviour
    {
        public GameObject hexPrefab;
        public TowerManager towerManager;

        [Header("Manual Alignment")]
        // X = horizontal spacing
        public float xSpacing = 1.0f;

        // NOTE: This is actually Z spacing (depth), not Y height
        public float ySpacing = 0.6495f;

        private float _hueSelection = 0f;

        private List<HexNode> _tableNodes = new List<HexNode>();

        public void GenerateTable(int rows, int cols, int floor)
        {
            _hueSelection = Random.value;

            // Clear old grid
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
                    // ✅ FIXED COORDINATE SYSTEM (XZ PLANE)
                    float xPos = (q * xSpacing) + xOffset;
                    float zPos = r * ySpacing;
                    float yPos = 0f;

                    Vector3 spawnPos = new Vector3(xPos, yPos, zPos);

                    GameObject go = Instantiate(
                        hexPrefab,
                        spawnPos,
                        hexPrefab.transform.rotation,
                        this.transform
                    );

                    go.name = $"Hex_F{floor}_R{r}_C{q}";

                    // Color distribution
                    MeshRenderer renderer = go.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null)
                    {
                        _hueSelection = (_hueSelection + 0.618033988749895f) % 1f;
                        renderer.material.color = Color.HSVToRGB(_hueSelection, 0.8f, 0.9f);
                    }

                    HexNode node = go.GetComponent<HexNode>();

                    if (node != null)
                    {
                        node.hexCoords = new Vector3Int(q, r, -q - r);
                        node.floorLevel = floor;
                        _tableNodes.Add(node);
                    }

                    if (towerManager != null)
                        towerManager.RegisterNode(q, r, floor, node);
                }
            }
        }

        public void TableTick()
        {
            Debug.Log("Table Calculation: Processing Next Day...");

            foreach (HexNode node in _tableNodes)
            {
                if (node != null)
                    node.ProcessTick();
            }
        }

        public void UnlockTableLevel(int level)
        {
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
