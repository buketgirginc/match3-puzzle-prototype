using UnityEngine;

namespace Match3.Core
{
    public class Cell
    {
        public Vector2Int Pos { get; }

        // Normal tile (Empty / Red / Blue / ...)
        public TileType Tile { get; set; }

        // --- STONE STATE (blocker) ---
        // Stone tile değildir; hücreyi geçilmez yapar
        public bool HasStone { get; set; }
        public int StoneHP { get; set; }

        public Cell(int x, int y)
        {
            Pos = new Vector2Int(x, y);

            // Default state
            HasStone = false;
            StoneHP = 0;
        }
    }
}
