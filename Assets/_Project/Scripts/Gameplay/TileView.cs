using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Cell _cell;

        public void Init(Cell cell, Sprite sprite)
        {
            _cell = cell;
            spriteRenderer.sprite = sprite;
        }

        public Cell Cell => _cell;


    }
}
