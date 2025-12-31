using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class StoneView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2Int GridPos { get; private set; }

        public void Init(Vector2Int gridPos, Sprite sprite)
        {
            GridPos = gridPos;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = (sprite != null);
                spriteRenderer.sprite = sprite;
            }
        }
    }
}
