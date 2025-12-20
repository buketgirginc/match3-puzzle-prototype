using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.Gameplay
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;

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

            TileView tile = RaycastTileUnderMouse();
            if (tile == null) return;

            HandleSelection(tile);
        }

        private TileView RaycastTileUnderMouse()
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();

            Vector3 worldPos = _cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0f)
            );

            Vector2 rayPos = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero);
            if (!hit.collider) return null;

            return hit.collider.GetComponent<TileView>();
        }

        private void HandleSelection(TileView tile)
        {
            // First click
            if (_firstSelected == null)
            {
                _firstSelected = tile;
                _firstSelected.SetSelected(true);
                Debug.Log($"First: {_firstSelected.Cell.Pos} ({_firstSelected.Cell.Tile})");
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
            TileView second = tile;

            // Adjacent? then swap
            if (AreAdjacent(_firstSelected.Cell.Pos, second.Cell.Pos))
            {
                Vector2Int a = _firstSelected.Cell.Pos;
                Vector2Int b = second.Cell.Pos;

                boardView.Board.SwapTiles(a, b); //data swap

                boardView.RefreshCell(a); //view refresh
                boardView.RefreshCell(b);

                var matches = boardView.Board.FindAllMatches(); //match check
                Debug.Log($"Matches after swap: {matches.Count}");

                _firstSelected.SetSelected(false); //selection reset
                _firstSelected = null;

                Debug.Log($"Swapped: {a} <-> {b}");
                return;
            }

            // Not adjacent: move selection to the new tile (better UX)
            _firstSelected.SetSelected(false);
            _firstSelected = second;
            _firstSelected.SetSelected(true);

            Debug.Log($"First moved to: {_firstSelected.Cell.Pos} ({_firstSelected.Cell.Tile})");
        }

        private bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) == 1;
        }
    }
}
