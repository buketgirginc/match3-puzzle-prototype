using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Match3.Gameplay
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private GameState gameState;
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
        {   //no moves left
            if (gameState != null && !gameState.CanSpendMove()) return;

            // Block input while feedback animation is playing or board is resolving
            if (_isAnimating) return;
            if (boardView != null && boardView.IsResolving) return;

            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            var hit = RaycastUnderMouse();

            if (hit.tile != null)
            {
                HandleSelection(hit.tile);
                return;
            }

            if (hit.stone != null)
            {
                HandleStoneClick(hit.stone);
                return;
            }

            // (Opsiyonel UX) Boş yere tıklayınca seçimi kaldır:
            if (_firstSelected != null && !_firstSelected.Equals(null))
            {
                _firstSelected.SetSelected(false);
                _firstSelected = null;
            }
        }

        private (TileView tile, StoneView stone) RaycastUnderMouse()
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();

            Vector3 worldPos = _cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0f)
            );

            Vector2 rayPos = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero);
            if (!hit.collider) return (null, null);

            // Tile?
            var tile = hit.collider.GetComponent<TileView>();
            if (tile != null) return (tile, null);

            // Stone?
            var stone = hit.collider.GetComponent<StoneView>();
            if (stone != null) return (null, stone);

            return (null, null);
        }

        private void HandleStoneClick(StoneView stone)
        {
            if (boardView == null) return;
            if (stone == null) return;

            // Eğer hiç seçili tile yoksa stone click'i yok say
            if (_firstSelected == null || _firstSelected.Equals(null)) return;

            Vector2Int a = _firstSelected.GridPos;
            Vector2Int b = stone.GridPos;

            // sadece adjacent ise nudge göster
            if (AreAdjacent(a, b))
            {
                // stone hücresinin world pozisyonunu hesapla (tile yok ama hedef noktaya doğru nudge yapacağız)
                Vector3 targetWorld = boardView.transform.TransformPoint(boardView.GridToLocal(b));
                StartCoroutine(PlayInvalidNudgeToPoint(_firstSelected, targetWorld));
            }

            _firstSelected.SetSelected(false);
            _firstSelected = null;
        }

        private void HandleSelection(TileView tile)
        {
            if (boardView == null) return;

            // Unity "destroyed object" safe-guard:
            // (Destroy edilmiş bir TileView referansı bazen != null görünür ama gerçekte null'dur.)
            if (_firstSelected != null && _firstSelected.Equals(null))
                _firstSelected = null;

            // First click
            if (_firstSelected == null)
            {
                _firstSelected = tile;
                _firstSelected.SetSelected(true);
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
            if (AreAdjacent(_firstSelected.GridPos, second.GridPos))
            {
                if (_isAnimating) return;
                if (boardView.IsResolving) return;

                Vector2Int a = _firstSelected.GridPos;
                Vector2Int b = second.GridPos;

                // --- STONE BLOCK: do not allow swap into stone cells ---
                var cellA = boardView.Board.Cells[a.x, a.y];
                var cellB = boardView.Board.Cells[b.x, b.y];

                if (cellA.HasStone || cellB.HasStone)
                {
                    StartCoroutine(PlayInvalidSwapFeedback(_firstSelected, second));

                    _firstSelected.SetSelected(false);
                    _firstSelected = null;
                    return;
                }

                // Same tile type? Don't swap
                if (_firstSelected.Type == second.Type)
                {
                    StartCoroutine(PlayInvalidSwapFeedback(_firstSelected, second));

                    _firstSelected.SetSelected(false);
                    _firstSelected = null;
                    return;
                }

                // 1) model swap (Board data)
                boardView.Board.SwapTiles(a, b);

                // 2) view swap (move objects)
                bool swappedViews = boardView.TrySwapViews(a, b);
                if (!swappedViews)
                {
                    // Extremely rare, but protects against desync during refactor.
                    // If view swap failed, undo model swap and bail.
                    boardView.Board.SwapTiles(a, b);

                    _firstSelected.SetSelected(false);
                    _firstSelected = null;
                    return;
                }

                // validate: did this swap create a match at either endpoint?
                bool createsMatch =
                    boardView.Board.CreatesMatchAt(a) ||
                    boardView.Board.CreatesMatchAt(b);

                if (!createsMatch)
                {
                    // swap back immediately (model)
                    boardView.Board.SwapTiles(a, b);

                    // swap back (view) - if this fails, model/view may desync; log it.
                    if (!boardView.TrySwapViews(a, b))
#if UNITY_EDITOR
                        Debug.LogWarning($"TrySwapViews failed while swapping back {a}<->{b}");
#endif
                    // invalid feedback (nudge)
                    StartCoroutine(PlayInvalidSwapFeedback(_firstSelected, second));

                }
                else
                {
                    if (gameState != null && gameState.CanSpendMove())
                        gameState.SpendMove();

                    StartCoroutine(ResolveBoardWithGravity());
                }

                // selection reset (always)
                _firstSelected.SetSelected(false);
                _firstSelected = null;
                return;
            }

            // Not adjacent: move selection to the new tile (better UX)
            _firstSelected.SetSelected(false);
            _firstSelected = second;
            _firstSelected.SetSelected(true);
        }

        private IEnumerator ResolveBoardWithGravity()
        {
            if (boardView.IsResolving) yield break;

            boardView.IsResolving = true;
#if UNITY_EDITOR
            Debug.Log("=== RESOLVE START ===");
            Debug.Log("Initial Board:\n" + boardView.Board.DebugPrint());
#endif
            int safety = 0;
            while (safety++ < 50)
            {
#if UNITY_EDITOR
                Debug.Log($"--- Cascade Step {safety} ---");
#endif

                // match detection
                var matches = boardView.Board.FindAllMatches();
                if (matches.Count == 0)
                {
#if UNITY_EDITOR
                    Debug.Log("No matches found. Resolve finished.");
#endif
                    break;
                }

                // 1) stone damage (adjacent matches)
                boardView.Board.ApplyAdjacentStoneDamage(matches, out var stonesHit, out var stonesBroken);
                boardView.PlayStoneHitFeedback(stonesHit, stonesBroken);

                // 2) objectives progress (tiles)
                if (gameState != null)
                    gameState.CollectFromMatches(matches, boardView.Board);

#if UNITY_EDITOR
                Debug.Log($"Matches found: {matches.Count}");
                Debug.Log("Board BEFORE clear:\n" + boardView.Board.DebugPrint());
#endif

                // clear anim (tiles)
                yield return StartCoroutine(boardView.AnimateClearAndRemove(matches));

                // clear data
                boardView.Board.ClearMatches(matches);
#if UNITY_EDITOR
                Debug.Log("Board AFTER clear:\n" + boardView.Board.DebugPrint());
#endif

                // Stone visuals update: broken stones disappear now
                if (stonesBroken.Count > 0)
                {
                    foreach (var p in stonesBroken)
                        boardView.RefreshCell(p);

                    //stoe objective progress
                    if (gameState != null)
                        gameState.CollectBrokenStones(stonesBroken.Count);
                }

                // gravity (animated) + model sync
                yield return StartCoroutine(boardView.AnimateGravityAndSyncModel());
#if UNITY_EDITOR
                Debug.Log("Board AFTER gravity:\n" + boardView.Board.DebugPrint());
#endif

                // refill
                int spawned = boardView.Board.RefillEmptiesAvoidImmediateMatches();
#if UNITY_EDITOR
                Debug.Log($"Refill done. Spawned tiles: {spawned}");
                Debug.Log("Board AFTER refill:\n" + boardView.Board.DebugPrint());
#endif
                yield return StartCoroutine(boardView.AnimateRefillDrop());
#if UNITY_EDITOR
                Debug.Log("Refill drop animation done.");
#endif

                yield return null;
            }

#if UNITY_EDITOR
            Debug.Log("=== RESOLVE END ===");
#endif
            boardView.IsResolving = false;
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

        private IEnumerator PlayInvalidNudgeToPoint(TileView first, Vector3 targetWorld)
        {
            _isAnimating = true;

            Transform t1 = first.transform;
            Vector3 p1 = t1.position;

            Vector3 dir = (targetWorld - p1).normalized;

            float half = invalidNudgeDuration * 0.5f;
            float t = 0f;

            while (t < half)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / half);
                t1.position = Vector3.Lerp(p1, p1 + dir * invalidNudgeDistance, a);
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / half);
                t1.position = Vector3.Lerp(p1 + dir * invalidNudgeDistance, p1, a);
                yield return null;
            }

            t1.position = p1;
            _isAnimating = false;
        }
    }
}
