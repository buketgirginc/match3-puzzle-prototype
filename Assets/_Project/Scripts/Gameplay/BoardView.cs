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

        [Header("Board Background")]
        [SerializeField] private SpriteRenderer boardBackground;
        [SerializeField] private float backgroundPadding = 0.5f;          // padding per side
        [SerializeField] private Vector2 backgroundExtra = Vector2.zero;  // fine-tune

        [Header("Clear Animation")]
        [SerializeField] private float clearDuration = 0.12f;

        private Board _board;

        // Maps grid position -> the instantiated TileView (only for NON-empty cells)
        private readonly Dictionary<Vector2Int, TileView> _viewsByPos = new();

        public Board Board => _board;
        public bool IsResolving { get; set; }


        public void Init(Board board)
        {
            _board = board;
            Render();
            LayoutBackground();
        }

        public Vector3 GridToWorld(Vector2Int pos)
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

            // snap transforms to new cell positions
            viewA.transform.position = GridToWorld(b);
            viewB.transform.position = GridToWorld(a);

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

            for (int x = 0; x < _board.Width; x++)
                for (int y = 0; y < _board.Height; y++)
                {
                    var cell = _board.Cells[x, y];

                    if (cell.Tile == TileType.Empty) //dont create tileview for empty cell
                        continue;

                    var tile = Instantiate(tilePrefab, transform);
                    tile.transform.position = new Vector3(x * cellSize, y * cellSize, 0f);

                    tile.Init(cell, null);
                    ApplyCellVisual(tile, cell.Tile);

                    _viewsByPos[cell.Pos] = tile;
                }
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

            // Center in world space (relative to BoardView transform)
            Vector3 center = transform.position + new Vector3(boardWidth * 0.5f, boardHeight * 0.5f, 0f);

            boardBackground.transform.position = center;

            // Sliced sprite size
            float targetW = boardWidth + (backgroundPadding * 2f) + backgroundExtra.x;
            float targetH = boardHeight + (backgroundPadding * 2f) + backgroundExtra.y;

            boardBackground.size = new Vector2(targetW, targetH);
        }

        public void RefreshCell(Vector2Int pos)
        {
            var cell = _board.Cells[pos.x, pos.y];
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
                view.transform.position = new Vector3(pos.x * cellSize, pos.y * cellSize, 0f);

                view.Init(cell, null);
                _viewsByPos[pos] = view;
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

    }
}
