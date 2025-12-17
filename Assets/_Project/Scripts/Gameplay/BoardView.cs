using System.Collections.Generic;
using UnityEngine;

namespace Match3.Gameplay
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Sprite[] tileSprites;
        [SerializeField] private float cellSize = 0.85f;

        private Board _board;

        // Maps grid position -> the instantiated TileView
        private readonly Dictionary<Vector2Int, TileView> _viewsByPos = new();

        public Board Board => _board;

        public void Init(Board board)
        {
            _board = board;
            Render();
        }

        private void Render()
        {
            // Clean old children (safe if you hit Play multiple times)
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

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
