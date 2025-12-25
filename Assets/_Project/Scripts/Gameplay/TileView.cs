using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D col;
        private Cell _cell;

        public Vector2Int GridPos { get; private set; }
        public TileType Type { get; private set; }
        private Vector3 _baseScale;
        private Color _baseColor;

        private void Awake()
        {
            if (!col) col = GetComponent<Collider2D>();

            _baseScale = transform.localScale;
            _baseColor = spriteRenderer.color;
        }

        public void SetGridPos(Vector2Int pos)
        {
            GridPos = pos;
        }

        public void SetType(TileType type)
        {
            Type = type;
        }
        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? _baseScale * 1.12f : _baseScale;
            spriteRenderer.color = selected ? _baseColor * 1.15f : _baseColor;
        }

        public void Init(Cell cell, Sprite sprite)
        {
            _cell = cell;

            SetGridPos(cell.Pos);
            SetType(cell.Tile);

            SetSprite(sprite);
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = sprite;

            if (col) col.enabled = true;
        }

        public void SetEmpty()
        {
            spriteRenderer.enabled = false;

            if (col) col.enabled = false;
            SetType(TileType.Empty);
        }

        public Cell Cell => _cell;
    }
}
