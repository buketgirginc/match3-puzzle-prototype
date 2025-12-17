using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Cell _cell;
        private Vector3 _baseScale;
        private Color _baseColor;


        private void Awake()
        {
            _baseScale = transform.localScale;
            _baseColor = spriteRenderer.color;
        }
        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? _baseScale * 1.12f : _baseScale;
            spriteRenderer.color = selected ? _baseColor * 1.15f : _baseColor;
        }
        public void Init(Cell cell, Sprite sprite)
        {
            _cell = cell;
            spriteRenderer.sprite = sprite;
        }
        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public Cell Cell => _cell;


    }
}
