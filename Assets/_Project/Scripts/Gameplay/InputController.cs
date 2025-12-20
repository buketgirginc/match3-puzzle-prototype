using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Match3.Gameplay
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private float invalidNudgeDistance = 0.12f;
        [SerializeField] private float invalidNudgeDuration = 0.12f;

        private bool _isAnimating;


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

            // Adjacent? then try swap
            if (AreAdjacent(_firstSelected.Cell.Pos, second.Cell.Pos))
            {
                if (_isAnimating) return;

                Vector2Int a = _firstSelected.Cell.Pos;
                Vector2Int b = second.Cell.Pos;

                boardView.Board.SwapTiles(a, b); //data swap

                boardView.RefreshCell(a); //view refresh after swap
                boardView.RefreshCell(b);

                //validate: did this swap create a match at either endpoint?
                bool createsMatch =
                boardView.Board.CreatesMatchAt(a) ||
                boardView.Board.CreatesMatchAt(b);

                Debug.Log($"Swapped: {a} <-> {b}| createsMatch={createsMatch}");

                if (!createsMatch)
                {
                    // swap back immediately (state should not change)
                    boardView.Board.SwapTiles(a, b);
                    boardView.RefreshCell(a);
                    boardView.RefreshCell(b);

                    // play invalid feedback (micro-nudge towards each other)
                    StartCoroutine(PlayInvalidSwapFeedback(_firstSelected, second));
                }


                _firstSelected.SetSelected(false); //selection reset (always)
                _firstSelected = null;


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

        private IEnumerator PlayInvalidSwapFeedback(TileView first, TileView second)
        {
            _isAnimating = true;

            Transform t1 = first.transform;
            Transform t2 = second.transform;

            Vector3 p1 = t1.position;
            Vector3 p2 = t2.position;

            Vector3 dir = (p2 - p1).normalized; // direction from first -> second

            float half = invalidNudgeDuration * 0.5f;
            float t = 0f;

            // phase 1: move slightly toward each other
            while (t < half)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / half);

                t1.position = Vector3.Lerp(p1, p1 + dir * invalidNudgeDistance, a);
                t2.position = Vector3.Lerp(p2, p2 - dir * invalidNudgeDistance, a);

                yield return null;
            }

            // phase 2: move back to original positions
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / half);

                t1.position = Vector3.Lerp(p1 + dir * invalidNudgeDistance, p1, a);
                t2.position = Vector3.Lerp(p2 - dir * invalidNudgeDistance, p2, a);

                yield return null;
            }

            // ensure perfect reset
            t1.position = p1;
            t2.position = p2;

            _isAnimating = false;
        }


    }
}
