using UnityEngine;
using Vertigro.Data;

namespace Vertigro.Logic
{
    public static class InsertDictionary
    {
        public static readonly int[,] Matrix = new int[,]
        {
            //   N   H   R   G   F  Hb  Fl   S
            {  0, +2, -2, +1, +2,  0, +1,  0 }, // NitrogenFixer
            { +2, -2, -2, +1, +1, -1, -1,  0 }, // HeavyFeeder
            { -1, -2, -2,  0, -2, -1, -1,  0 }, // Rooter
            { +2, -2, -1,  0, +1, +1,  0, -2 }, // Greens
            { +1, -2, -2, +1,  0, +2, +2, -2 }, // FruitBearer
            { +1, -1, -1, +2, +2, -2, +1, -2 }, // Herb
            { +1, -1, -1, +1, +2, +1,  0, -2 }, // Flower
            {  0, +1, -1, -2, -2, -2, -2, -2 }  // ShadeMaker
        };

        public static int GetAdjacencyBonus(PlantTag host, PlantTag neighbor)
        {
            int row = (int)host;
            int col = (int)neighbor;
            return Matrix[row, col];
        }
    }
}