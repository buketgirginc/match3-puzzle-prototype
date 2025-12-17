using Match3.Core;
using System.Text;
using UnityEngine;

namespace Match3.Gameplay
{
    public class Board
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Cell[,] Cells { get; private set; }

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;

            Cells = new Cell[Width, Height]; //boş grid

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell(x, y);
        }

        public void FillRandom()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y].Tile = (TileType)Random.Range(0, 4);
        }

        public void SwapTiles(Vector2Int a, Vector2Int b)
        {
            var cellA = Cells[a.x, a.y];
            var cellB = Cells[b.x, b.y];

            (cellA.Tile, cellB.Tile) = (cellB.Tile, cellA.Tile);
        }
        public string DebugPrint()
        {
            var sb = new StringBuilder();

            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    char c = Cells[x, y].Tile switch
                    {
                        TileType.Red => 'R',
                        TileType.Blue => 'B',
                        TileType.Green => 'G',
                        TileType.Yellow => 'Y',
                        _ => '?'
                    };
                    sb.Append(c).Append(' ');
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}
