using System.Collections.Generic;
using UnityEngine;

namespace Vertigro.Logic
{
    public class TowerManager : MonoBehaviour
    {
        // Vector3Int(q, r, floor) is our unique key
        public Dictionary<Vector3Int, HexNode> Grid = new Dictionary<Vector3Int, HexNode>();

        public void RegisterNode(int q, int r, int floor, HexNode node)
        {
            Vector3Int key = new Vector3Int(q, r, floor);
            if (!Grid.ContainsKey(key))
            {
                Grid.Add(key, node);
                node.name = $"Hex_{q}_{r}_F{floor}"; // AAA naming convention
            }
        }

        public HexNode GetNodeAt(int q, int r, int floor)
        {
            Vector3Int key = new Vector3Int(q, r, floor);
            return Grid.TryGetValue(key, out HexNode node) ? node : null;
        }
    }
}