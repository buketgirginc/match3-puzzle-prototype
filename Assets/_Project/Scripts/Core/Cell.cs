using UnityEngine;

namespace Match3.Core
{
    public class Cell
    {
        public Vector2Int Pos { get; }
        public TileType Tile { get; set; }

        public Cell(int x, int y)
        {
            Pos = new Vector2Int(x, y);
        }
    }
}
