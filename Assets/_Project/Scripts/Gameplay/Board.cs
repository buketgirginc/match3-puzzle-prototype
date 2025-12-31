using Match3.Core;
using System.Collections.Generic;
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

        private TileType GetRandomColorTile()
        {
            // Assumes: Empty=0, colors start at 1 (Red..Yellow)
            return (TileType)Random.Range(1, 5); // 1..4
        }

        public void FillRandom()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y].Tile = GetRandomColorTile();
        }

        public void FillRandomNoMatches()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TileType t;
                    do
                    {
                        t = GetRandomColorTile();
                        Cells[x, y].Tile = t;
                    }
                    while (CreatesMatchAt(new Vector2Int(x, y)));
                }
            }
        }

        public void ClearMatches(IEnumerable<Vector2Int> matches)
        {
            foreach (var p in matches)
                Cells[p.x, p.y].Tile = TileType.Empty;
        }

        public void SwapTiles(Vector2Int a, Vector2Int b)
        {
            var cellA = Cells[a.x, a.y];
            var cellB = Cells[b.x, b.y];

            (cellA.Tile, cellB.Tile) = (cellB.Tile, cellA.Tile);
        }

        public List<Vector2Int> FindHorizontalMatches(int minLength = 3)
        {
            var matches = new List<Vector2Int>();

            for (int y = 0; y < Height; y++)
            {
                int runStartX = 0;
                int runLength = 1;

                for (int x = 1; x < Width; x++)
                {
                    var prev = Cells[x - 1, y].Tile;
                    var curr = Cells[x, y].Tile;

                    // ignore empties
                    if (curr != TileType.Empty && curr == prev)
                    {
                        runLength++;
                    }
                    else
                    {
                        // run ended at x-1
                        if (prev != TileType.Empty && runLength >= minLength)
                        {
                            for (int k = 0; k < runLength; k++)
                                matches.Add(new Vector2Int(runStartX + k, y));
                        }

                        // start new run at x
                        runStartX = x;
                        runLength = 1;
                    }
                }

                // end-of-row run check (important!)
                var last = Cells[Width - 1, y].Tile;
                if (last != TileType.Empty && runLength >= minLength)
                {
                    for (int k = 0; k < runLength; k++)
                        matches.Add(new Vector2Int(runStartX + k, y));
                }
            }

            return matches;
        }

        public List<Vector2Int> FindVerticalMatches(int minLength = 3)
        {
            var matches = new List<Vector2Int>();

            for (int x = 0; x < Width; x++)
            {
                int runStartY = 0;
                int runLength = 1;

                for (int y = 1; y < Height; y++)
                {
                    var prev = Cells[x, y - 1].Tile;
                    var curr = Cells[x, y].Tile;

                    // ignore empties
                    if (curr != TileType.Empty && curr == prev)
                    {
                        runLength++;
                    }
                    else
                    {
                        // run ended at y-1
                        if (prev != TileType.Empty && runLength >= minLength)
                        {
                            for (int k = 0; k < runLength; k++)
                                matches.Add(new Vector2Int(x, runStartY + k));
                        }

                        // start new run at y
                        runStartY = y;
                        runLength = 1;
                    }
                }

                // end-of-column run check
                var last = Cells[x, Height - 1].Tile;
                if (last != TileType.Empty && runLength >= minLength)
                {
                    for (int k = 0; k < runLength; k++)
                        matches.Add(new Vector2Int(x, runStartY + k));
                }
            }

            return matches;
        }

        public HashSet<Vector2Int> FindAllMatches(int minLength = 3)
        {
            var result = new HashSet<Vector2Int>();

            foreach (var p in FindHorizontalMatches(minLength))
                result.Add(p);

            foreach (var p in FindVerticalMatches(minLength))
                result.Add(p);

            return result;
        }

        public bool CreatesMatchAt(Vector2Int pos, int minLength = 3)
        {
            var type = Cells[pos.x, pos.y].Tile;
            if (type == TileType.Empty) return false;

            // Horizontal run through pos
            int count = 1;

            // left
            for (int x = pos.x - 1; x >= 0; x--)
            {
                if (Cells[x, pos.y].Tile != type) break; //dont count when you see a different color
                count++;
            }

            // right
            for (int x = pos.x + 1; x < Width; x++)
            {
                if (Cells[x, pos.y].Tile != type) break;
                count++;
            }

            if (count >= minLength) return true;

            // Vertical run through pos
            count = 1;

            // down
            for (int y = pos.y - 1; y >= 0; y--)
            {
                if (Cells[pos.x, y].Tile != type) break;
                count++;
            }

            // up
            for (int y = pos.y + 1; y < Height; y++)
            {
                if (Cells[pos.x, y].Tile != type) break;
                count++;
            }

            return count >= minLength;
        }

        public void ApplyAdjacentStoneDamage(
                   IEnumerable<Vector2Int> clearedCells,
                   out HashSet<Vector2Int> stonesHit,
                   out HashSet<Vector2Int> stonesBroken)
        {
            stonesHit = new HashSet<Vector2Int>();
            stonesBroken = new HashSet<Vector2Int>();

            // 4-directional adjacency
            Vector2Int[] dirs = new[]
            {
                new Vector2Int( 1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int( 0, 1),
                new Vector2Int( 0,-1),
            };

            foreach (var p in clearedCells)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int n = p + dirs[i];
                    if (!InBounds(n)) continue;

                    var cell = Cells[n.x, n.y];
                    if (!cell.HasStone) continue;

                    // multi-hit allowed: each adjacent cleared cell deals 1 damage
                    cell.StoneHP -= 1;
                    stonesHit.Add(n);

                    if (cell.StoneHP <= 0)
                    {
                        cell.HasStone = false;
                        cell.StoneHP = 0;
                        cell.Tile = TileType.Empty; // guarantee
                        stonesBroken.Add(n);
                    }
                }
            }
        }

        private bool InBounds(Vector2Int p)
        {
            return p.x >= 0 && p.x < Width && p.y >= 0 && p.y < Height;
        }

        public bool ApplyGravity()
        {
            bool moved = false;

            for (int x = 0; x < Width; x++) //apply for every column
            {
                // writeY: Bu kolonda bir sonraki "dolu" taşın ineceği hedef y index'i.
                int writeY = 0;

                for (int readY = 0; readY < Height; readY++)  // readY: Kolonu aşağıdan yukarı tarar, dolu taşları bulur.
                {
                    var t = Cells[x, readY].Tile;
                    if (t == TileType.Empty) continue; //hücre boşsa geç dolu taş arıyoruz

                    if (readY != writeY) //taş yukarıdaysa onu writeY konumuna indir
                    {
                        Cells[x, writeY].Tile = t;
                        Cells[x, readY].Tile = TileType.Empty; //eski yeri bosalt
                        moved = true;
                    }

                    writeY++; //Bir sonraki dolu taş, bir üstteki boş yere inecek
                }
            }

            return moved;
        }

        public int RefillEmptiesAvoidImmediateMatches(int maxTriesPerCell = 20)
        {
            int spawned = 0; //kaç hücreye yeni tile koyduk?

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y].HasStone) continue; //stone ise spawn yok

                    if (Cells[x, y].Tile != TileType.Empty) continue; // cell doluysa spawn yok

                    Vector2Int pos = new Vector2Int(x, y);

                    TileType t = TileType.Red; //baslagıctaki renk önemli değil
                    int tries = 0;

                    do
                    {
                        t = GetRandomColorTile();
                        Cells[x, y].Tile = t;
                        tries++;
                    }
                    while (tries < maxTriesPerCell && CreatesMatchAt(pos));

                    // (çok nadir) max tries dolarsa o anki tile’ı bırakırız
                    spawned++;
                }
            }

            return spawned;
        }
        public string DebugPrint()
        {
            var sb = new StringBuilder();

            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = Cells[x, y];

                    char c =
                        cell.HasStone ? 'S' :
                        cell.Tile switch
                        {
                            TileType.Empty => '.',
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
