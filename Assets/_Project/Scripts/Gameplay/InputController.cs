using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.Gameplay
{
    public class InputController : MonoBehaviour
    {
        private Camera _cam;
        private TileView _firstSelected;

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
            // First click
            if (_firstSelected == null)
            {
                _firstSelected = tile;
                _firstSelected.SetSelected(true);
                Debug.Log($"First: {_firstSelected.Cell.Pos}");
                return;
            }

            // Clicking the same tile again -> deselect
            if (_firstSelected == tile)
            {
                _firstSelected.SetSelected(false);
                _firstSelected = null;
                return;
            }

            // Second click
            var second = tile;

            // Check adjacency
            if (AreAdjacent(_firstSelected.Cell.Pos, second.Cell.Pos))
            {
                Debug.Log($"Swap attempt: {_firstSelected.Cell.Pos} <-> {second.Cell.Pos}");
                // TODO (next step): actually swap + animate
                _firstSelected.SetSelected(false);
                _firstSelected = null;
                return;
            }

            // Not adjacent: move selection to the new tile (better UX)
            _firstSelected.SetSelected(false);
            _firstSelected = second;
            _firstSelected.SetSelected(true);

            Debug.Log($"First moved to: {_firstSelected.Cell.Pos}");
        }
        private bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) == 1; // exactly one step horizontally or vertically
        }

    }
}
