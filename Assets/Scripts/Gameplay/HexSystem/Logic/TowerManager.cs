using System.Collections.Generic;
using UnityEngine;

namespace Vertigro.Logic
{
    public class TowerManager : MonoBehaviour
    {
        public const string DefaultShelfId = "RackSlot_1";

        public Dictionary<ShelfGridKey, HexNode> Grid = new Dictionary<ShelfGridKey, HexNode>();

        public static string NormalizeShelfId(string shelfId)
        {
            return string.IsNullOrWhiteSpace(shelfId) ? DefaultShelfId : shelfId.Trim();
        }

        public void RegisterNode(string shelfId, int q, int r, int floor, HexNode node)
        {
            if (node == null)
            {
                Debug.LogWarning($"TowerManager: Cannot register null node for shelf {NormalizeShelfId(shelfId)} at ({q}, {r}, floor {floor}).");
                return;
            }

            string normalizedShelfId = NormalizeShelfId(shelfId);
            ShelfGridKey key = new ShelfGridKey(normalizedShelfId, q, r, floor);

            if (Grid.ContainsKey(key))
                Debug.LogWarning($"TowerManager: Replacing existing node registration for {key}.");

            Grid[key] = node;
            node.name = $"{normalizedShelfId}_Hex_{q}_{r}_F{floor}";
        }

        public void RegisterNode(int q, int r, int floor, HexNode node)
        {
            RegisterNode(DefaultShelfId, q, r, floor, node);
        }

        public HexNode GetNodeAt(string shelfId, int q, int r, int floor)
        {
            ShelfGridKey key = new ShelfGridKey(shelfId, q, r, floor);
            return Grid.TryGetValue(key, out HexNode node) ? node : null;
        }

        public HexNode GetNodeAt(int q, int r, int floor)
        {
            return GetNodeAt(DefaultShelfId, q, r, floor);
        }

        public IEnumerable<HexNode> GetShelfNodes(string shelfId)
        {
            string normalizedShelfId = NormalizeShelfId(shelfId);

            foreach (var entry in Grid)
            {
                if (entry.Key.ShelfId == normalizedShelfId)
                    yield return entry.Value;
            }
        }

        public void ClearShelfGrid(string shelfId)
        {
            string normalizedShelfId = NormalizeShelfId(shelfId);
            List<ShelfGridKey> keysToRemove = new List<ShelfGridKey>();

            foreach (ShelfGridKey key in Grid.Keys)
            {
                if (key.ShelfId == normalizedShelfId)
                    keysToRemove.Add(key);
            }

            foreach (ShelfGridKey key in keysToRemove)
                Grid.Remove(key);
        }

        public void ClearGrid()
        {
            Grid.Clear();
        }
    }

    public struct ShelfGridKey : System.IEquatable<ShelfGridKey>
    {
        public string ShelfId { get; }
        public int Q { get; }
        public int R { get; }
        public int Floor { get; }

        public ShelfGridKey(string shelfId, int q, int r, int floor)
        {
            ShelfId = TowerManager.NormalizeShelfId(shelfId);
            Q = q;
            R = r;
            Floor = floor;
        }

        public bool Equals(ShelfGridKey other)
        {
            return ShelfId == other.ShelfId
                   && Q == other.Q
                   && R == other.R
                   && Floor == other.Floor;
        }

        public override bool Equals(object obj)
        {
            return obj is ShelfGridKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ShelfId.GetHashCode();
                hash = hash * 31 + Q;
                hash = hash * 31 + R;
                hash = hash * 31 + Floor;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{ShelfId}:({Q}, {R}, F{Floor})";
        }
    }
}
