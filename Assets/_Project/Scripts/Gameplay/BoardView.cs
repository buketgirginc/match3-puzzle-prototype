using System.Collections.Generic;
using UnityEngine;

namespace Match3.Gameplay
{
    public class BoardView : MonoBehaviour
    {
        [Header("Tiles")]
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Sprite[] tileSprites;
        [SerializeField] private float cellSize = 0.85f;

        [Header("Board Background (optional)")]
        [SerializeField] private SpriteRenderer boardBackground;
        [SerializeField] private float backgroundPadding = 0.5f;          // padding per side
        [SerializeField] private Vector2 backgroundExtra = Vector2.zero;  // fine-tune
        private Board _board;

        // Maps grid position -> the instantiated TileView
        private readonly Dictionary<Vector2Int, TileView> _viewsByPos = new();

        public Board Board => _board;

        public void Init(Board board)
        {
            _board = board;
            Render();
            LayoutBackground();
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

                    var tile = Instantiate(tilePrefab, transform);
                    tile.transform.position = new Vector3(x * cellSize, y * cellSize, 0f);

                    tile.Init(cell, tileSprites[(int)cell.Tile]);

                    _viewsByPos[cell.Pos] = tile;
                }
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

            if (_viewsByPos.TryGetValue(pos, out var view))
            {
                view.SetSprite(tileSprites[(int)cell.Tile]);
            }
            else
            {
                Debug.LogWarning($"No TileView found at {pos} to refresh.");
            }
        }
    }
}
