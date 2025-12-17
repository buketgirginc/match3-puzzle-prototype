using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.Gameplay
{
    public class InputController : MonoBehaviour
    {
        private Camera _cam;
        private TileView _selected;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            // Screen -> World
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0f)
            );

            Vector2 rayPos = new Vector2(worldPos.x, worldPos.y);

            // Raycast
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero);
            if (!hit.collider) return;

            TileView tile = hit.collider.GetComponent<TileView>();
            if (tile == null) return;

            HandleSelection(tile);
        }

        private void HandleSelection(TileView tile)
        {
            // Same tile clicked -> deselect
            if (_selected == tile)
            {
                _selected.SetSelected(false);
                _selected = null;
                return;
            }

            // Deselect previous
            if (_selected != null)
                _selected.SetSelected(false);

            // Select new
            _selected = tile;
            _selected.SetSelected(true);

            Debug.Log($"Selected: {_selected.Cell.Pos} ({_selected.Cell.Tile})");
        }
    }
}
