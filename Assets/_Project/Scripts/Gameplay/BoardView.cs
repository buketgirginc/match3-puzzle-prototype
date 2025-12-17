using UnityEngine;

namespace Match3.Gameplay
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Sprite[] tileSprites;
        [SerializeField] private float cellSize = 0.85f;
        private Board _board;

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

            for (int x = 0; x < _board.Width; x++)
                for (int y = 0; y < _board.Height; y++)
                {
                    var cell = _board.Cells[x, y];

                    var tile = Instantiate(tilePrefab, transform);
                    tile.transform.position = new Vector3(x * cellSize, y * cellSize, 0);

                    tile.Init(cell, tileSprites[(int)cell.Tile]);
                }
        }
    }
}
