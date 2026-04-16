using UnityEngine;
using Vertigrow.Data; // Allows us to see the PlantTag enum

namespace Vertigrow.Logic
{
    public static class AdjacencyDictionary
    {
        // This is your matrix. 'readonly' means it cannot be accidentally changed while playing.
        public static readonly int[,] Matrix = new int[,]
        {
            //  N   H   R   G   F  Hb  Fl   S
            {  0, +2, -2, +1, +2,  0, +1,  0 }, // NitrogenFixer
            { +2, -2, -2, +1, +1, -1, -1,  0 }, // HeavyFeeder
            { -1, -2, -2,  0, -2, -1, -1,  0 }, // Rooter
            { +2, -2, -1,  0, +1, +1,  0, -2 }, // Greens
            { +1, -2, -2, +1,  0, +2, +2, -2 }, // FruitBearer
            { +1, -1, -1, +2, +2, -2, +1, -2 }, // Herb
            { +1, -1, -1, +1, +2, +1,  0, -2 }, // Flower
            {  0, +1, -1, -2, -2, -2, -2, -2 }  // ShadeMaker
        };

        /// <summary>
        /// Instantly looks up the bonus between two tags.
        /// </summary>
        public static int GetBonus(PlantTag myTag, PlantTag neighborTag)
        {
            // We convert the Enums into numbers (0-7) to find the exact row and column
            int row = (int)myTag;
            int col = (int)neighborTag;

            return Matrix[row, col];
        }
    }
}