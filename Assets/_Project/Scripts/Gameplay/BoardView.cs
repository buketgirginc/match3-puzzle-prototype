using System.Collections;
using System.Collections.Generic;
using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class BoardView : MonoBehaviour
    {
        [Header("Tiles")]
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Sprite[] tileSprites; // R,B,G,Y (4 items)
        [SerializeField] private float cellSize = 0.85f;

        [Header("Stones")]
        [SerializeField] private StoneView stonePrefab;
        [SerializeField] private Sprite stoneSprite;

        [Header("Board Background")]
        [SerializeField] private SpriteRenderer boardBackground;
        [SerializeField] private float backgroundPadding = 0.5f;          // padding per side
        [SerializeField] private Vector2 backgroundExtra = Vector2.zero;  // fine-tune

        [Header("Clear Animation")]
        [SerializeField] private float clearDuration = 0.12f;

        [Header("Gravity Animation")]
        [SerializeField] private float gravityDuration = 0.12f;

        [Header("Refill Animation")]
        [SerializeField] private float refillDropDuration = 0.14f;
        [SerializeField] private int spawnRowOffset = 2; // kaç hücre yukarıdan spawn edilsin


        private Board _board;

        // Maps grid position -> the instantiated TileView (only for NON-empty cells)
        private readonly Dictionary<Vector2Int, TileView> _viewsByPos = new();

        // Maps grid position -> instantiated StoneView
        private readonly Dictionary<Vector2Int, StoneView> _stoneViewsByPos = new();

        public Board Board => _board;
        public bool IsResolving { get; set; }


        public void Init(Board board)
        {
            _board = board;
            Render();
            LayoutBackground();
        }

        // Grid koordinatlarını BoardView LOCAL SPACE'ine çevirir
        public Vector3 GridToLocal(Vector2Int pos)
        {
            return new Vector3(pos.x * cellSize, pos.y * cellSize, 0f);
        }

        public bool TrySwapViews(Vector2Int a, Vector2Int b)
        {
            if (!_viewsByPos.TryGetValue(a, out var viewA) || viewA == null) return false;
            if (!_viewsByPos.TryGetValue(b, out var viewB) || viewB == null) return false;

            // swap dictionary mapping (which TileView belongs to which grid pos)
            _viewsByPos[a] = viewB;
            _viewsByPos[b] = viewA;

            viewA.SetGridPos(b);
            viewB.SetGridPos(a);

            // snap transforms to new local cell positions
            viewA.transform.localPosition = GridToLocal(b);
            viewB.transform.localPosition = GridToLocal(a);

            return true;
        }
        private void Render()
        {
            // Clean old children (safe if you hit Play multiple times)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);

                if (boardBackground != null && child == boardBackground.transform)
                    continue;

                Destroy(child.gameObject);
            }

            _viewsByPos.Clear();
            _stoneViewsByPos.Clear();

            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    var cell = _board.Cells[x, y];
                    Vector2Int pos = cell.Pos;

                    // 1) Stone overlay (no tile in this cell)
                    if (cell.HasStone)
                    {
                        CreateOrReplaceStone(pos);
                        continue;
                    }

                    // 2) Normal tile (skip empties)
                    if (cell.Tile == TileType.Empty)
                        continue;

                    var tile = Instantiate(tilePrefab, transform);
                    tile.transform.localPosition = GridToLocal(pos);

                    tile.Init(cell, null);
                    ApplyCellVisual(tile, cell.Tile);

                    _viewsByPos[pos] = tile;
                }
            }
        }


        private void CreateOrReplaceStone(Vector2Int pos)
        {
            if (stonePrefab == null) return;

            var stone = Instantiate(stonePrefab, transform);
            stone.transform.localPosition = GridToLocal(pos);
            stone.Init(pos, stoneSprite);

            _stoneViewsByPos[pos] = stone;
        }


        private void ApplyCellVisual(TileView view, TileType tileType)
        {
            view.SetType(tileType);

            if (tileType == TileType.Empty)
            {
                view.SetEmpty();
                return;
            }

            // TileType: Red=1..Yellow=4 ; tileSprites: 0..3
            view.SetSprite(tileSprites[(int)tileType - 1]);
        }

        private void LayoutBackground()
        {
            if (boardBackground == null || _board == null) return;

            // Board spans from (0,0) to ((W-1)*cellSize, (H-1)*cellSize)
            float boardWidth = (_board.Width - 1) * cellSize;
            float boardHeight = (_board.Height - 1) * cellSize;

            // Center in local space
            Vector3 center = new Vector3(boardWidth * 0.5f, boardHeight * 0.5f, 0f);
            boardBackground.transform.localPosition = center;

            // Sliced sprite size
            float targetW = boardWidth + (backgroundPadding * 2f) + backgroundExtra.x;
            float targetH = boardHeight + (backgroundPadding * 2f) + backgroundExtra.y;

            boardBackground.size = new Vector2(targetW, targetH);
        }

        public void RefreshCell(Vector2Int pos)
        {
            var cell = _board.Cells[pos.x, pos.y];

            // --- STONE CELL ---
            if (cell.HasStone)
            {
                // Ensure no tile view exists here
                if (_viewsByPos.TryGetValue(pos, out var tv) && tv != null)
                {
                    Destroy(tv.gameObject);
                    _viewsByPos.Remove(pos);
                }

                // Ensure stone view exists
                if (!_stoneViewsByPos.TryGetValue(pos, out var sv) || sv == null)
                {
                    CreateOrReplaceStone(pos);
                }
                else
                {
                    sv.transform.localPosition = GridToLocal(pos);
                }

                return;
            }

            // --- NON-STONE CELL: ensure stone view removed ---
            if (_stoneViewsByPos.TryGetValue(pos, out var existingStone) && existingStone != null)
            {
                Destroy(existingStone.gameObject);
                _stoneViewsByPos.Remove(pos);
            }

            bool shouldHaveTile = cell.Tile != TileType.Empty;
            bool hasView = _viewsByPos.TryGetValue(pos, out var view);

            if (!shouldHaveTile)
            {
                if (hasView && view != null)
                {
                    Destroy(view.gameObject);
                    _viewsByPos.Remove(pos);
                }
                return;
            }
            //shouldHaveTile==true
            if (!hasView || view == null)
            {
                view = Instantiate(tilePrefab, transform);
                view.transform.localPosition = GridToLocal(pos);

                view.Init(cell, null);
                _viewsByPos[pos] = view;
            }
            else
            {
                // Keep it snapped to the correct local slot (protects against any desync)
                view.transform.localPosition = GridToLocal(pos);
                view.SetGridPos(pos);
            }

            ApplyCellVisual(view, cell.Tile); //update the sprite

        }

        public void RefreshAll()
        {
            for (int x = 0; x < _board.Width; x++)
                for (int y = 0; y < _board.Height; y++)
                    RefreshCell(new Vector2Int(x, y));
        }


        public IEnumerator AnimateClearAndRemove(IEnumerable<Vector2Int> positions)
        {
            var tiles = new List<TileView>();

            // Collect TileViews to clear
            foreach (var pos in positions)
            {
                if (_viewsByPos.TryGetValue(pos, out var view) && view != null)
                    tiles.Add(view);
            }

            // Store original scales
            var original = new Vector3[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
                original[i] = tiles[i].transform.localScale;

            float t = 0f;

            // Scale-down animation
            while (t < clearDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / clearDuration);
                float s = Mathf.Lerp(1f, 0.2f, a);

                for (int i = 0; i < tiles.Count; i++)
                    tiles[i].transform.localScale = original[i] * s;

                yield return null;
            }

            // FINAL STEP: remove tiles from scene + dictionary
            for (int i = 0; i < tiles.Count; i++)
            {
                TileView tile = tiles[i];
                Vector2Int pos = tile.GridPos;

                // Remove mapping first
                if (_viewsByPos.ContainsKey(pos))
                    _viewsByPos.Remove(pos);

                Destroy(tile.gameObject);
            }
        }

        public IEnumerator AnimateGravityAndSyncModel()
        {
            if (_board == null) yield break;
            var newMap = new Dictionary<Vector2Int, TileView>(_viewsByPos.Count); //ayni tile sayisina sahip yeni bir mapping oluştur

            var tiles = new List<TileView>(_viewsByPos.Count); //Her tile için start/end pozisyonları
            var startPos = new List<Vector3>(_viewsByPos.Count);
            var endPos = new List<Vector3>(_viewsByPos.Count);

            int H = _board.Height;

            for (int x = 0; x < _board.Width; x++)
            {
                // 1) collect blocked Y (stones) in this column
                var blockedYs = new List<int>();
                for (int y = 0; y < H; y++)
                {
                    if (_board.Cells[x, y].HasStone)
                        blockedYs.Add(y);
                }
                blockedYs.Sort();

                // Helper: process a segment [segStart..segEnd]
                void ProcessSegment(int segStart, int segEnd)
                {
                    if (segEnd < segStart) return;

                    // Collect tile views currently inside this segment
                    var segmentTiles = new List<TileView>();

                    foreach (var kvp in _viewsByPos)
                    {
                        Vector2Int p = kvp.Key;
                        if (p.x != x) continue;

                        var tv = kvp.Value;
                        if (tv == null) continue;

                        int y0 = tv.GridPos.y;
                        if (y0 < segStart || y0 > segEnd) continue;

                        segmentTiles.Add(tv);
                    }

                    // sort bottom -> top
                    segmentTiles.Sort((a, b) => a.GridPos.y.CompareTo(b.GridPos.y));

                    int writeY = segStart;

                    for (int i = 0; i < segmentTiles.Count; i++)
                    {
                        var tile = segmentTiles[i];

                        Vector2Int from = tile.GridPos;
                        Vector2Int to = new Vector2Int(x, writeY);

                        // Safety: never target a stone cell (shouldn't happen if seg excludes stones)
                        if (_board.Cells[x, writeY].HasStone)
                        {
                            // Find next free slot inside segment
                            int yy = writeY + 1;
                            while (yy <= segEnd && _board.Cells[x, yy].HasStone) yy++;
                            if (yy > segEnd) break;
                            to = new Vector2Int(x, yy);
                            writeY = yy;
                        }

                        Vector3 fromL = tile.transform.localPosition;
                        Vector3 toL = GridToLocal(to);

                        newMap[to] = tile;
                        tile.SetGridPos(to);

                        tiles.Add(tile);
                        startPos.Add(fromL);
                        endPos.Add(toL);

                        writeY++;
                        if (writeY > segEnd) break;
                    }
                }

                // 2) walk segments separated by stones
                int currentStart = 0;

                for (int i = 0; i < blockedYs.Count; i++)
                {
                    int stoneY = blockedYs[i];
                    int segEnd = stoneY - 1;

                    ProcessSegment(currentStart, segEnd);

                    currentStart = stoneY + 1;
                }

                // final segment above last stone
                ProcessSegment(currentStart, H - 1);
            }

            // 3) yeni mappingi aktif hale getir
            _viewsByPos.Clear();
            foreach (var kvp in newMap)
                _viewsByPos[kvp.Key] = kvp.Value;

            // 4) Animasyon: tüm tile'ları aynı anda hedefe lerp et
            float t = 0f;
            float dur = Mathf.Max(0.0001f, gravityDuration);

            while (t < dur)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / dur);

                for (int i = 0; i < tiles.Count; i++)
                    tiles[i].transform.localPosition = Vector3.Lerp(startPos[i], endPos[i], a);

                yield return null;
            }

            // Snap final (kesin pozisyona kilitliyoruz)
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].transform.localPosition = endPos[i];

            // 5) MODEL SYNC: reset ONLY Tile types (do not touch stone state)
            for (int x = 0; x < _board.Width; x++)
                for (int y = 0; y < _board.Height; y++)
                    _board.Cells[x, y].Tile = TileType.Empty;

            foreach (var kvp in _viewsByPos)
            {
                Vector2Int p = kvp.Key;
                TileView tile = kvp.Value;

                // extra safety: don't write into stone cells
                if (_board.Cells[p.x, p.y].HasStone) continue;

                _board.Cells[p.x, p.y].Tile = tile.Type;
            }

        }

        public IEnumerator AnimateRefillDrop()
        {
            var tiles = new List<TileView>();
            var startPos = new List<Vector3>();
            var endPos = new List<Vector3>();

            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    var cell = _board.Cells[x, y];
                    if (cell.Tile == TileType.Empty) continue;
                    if (cell.HasStone) continue; // safety: stone cells should never have tiles

                    Vector2Int pos = new Vector2Int(x, y);

                    if (_viewsByPos.ContainsKey(pos)) continue;

                    // Spawn: kolonun üstünden başlat
                    Vector2Int spawnGrid = new Vector2Int(x, _board.Height + spawnRowOffset);
                    Vector3 spawnLocal = GridToLocal(spawnGrid);
                    Vector3 targetLocal = GridToLocal(pos);

                    var view = Instantiate(tilePrefab, transform);
                    view.transform.localPosition = spawnLocal;

                    // TileView metadata + visuals
                    view.SetGridPos(pos);
                    view.SetType(cell.Tile);
                    ApplyCellVisual(view, cell.Tile);

                    // Mapping'e ekle
                    _viewsByPos[pos] = view;

                    tiles.Add(view);
                    startPos.Add(spawnLocal);
                    endPos.Add(targetLocal);
                }
            }

            if (tiles.Count == 0)
                yield break;

            float t = 0f;
            float dur = Mathf.Max(0.0001f, refillDropDuration);

            while (t < dur)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / dur);

                for (int i = 0; i < tiles.Count; i++)
                    tiles[i].transform.localPosition = Vector3.Lerp(startPos[i], endPos[i], a);

                yield return null;
            }

            // Snap final
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].transform.localPosition = endPos[i];
        }
    }
}
