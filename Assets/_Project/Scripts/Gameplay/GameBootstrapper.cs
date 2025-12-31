using Match3.Core;
using Match3.Gameplay;
using Match3.Levels;
using UnityEngine;

namespace Match3
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private GameState gameState;
        [SerializeField] private LevelManager levelManager;

        private Board _board;

        void Awake()
        {
            if (levelManager != null && levelManager.CurrentLevel == null)
                levelManager.StartAt(0);

            LoadCurrentLevel();
        }
        public void LoadCurrentLevel()
        {
            var level = levelManager != null ? levelManager.CurrentLevel : null;
            if (level == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[GameBootstrapper] No LevelConfig found.");
#endif

                return;
            }

            // 1) model board
            _board = new Board();
            _board.Initialize(level.width, level.height);
            _board.FillRandomNoMatches();

            // Apply stones (model-only)
            ApplyStonesFromLevelConfig(level, _board);

            // 2) view
            boardView.Init(_board);

            // 3) state
            gameState.Init(level);
        }

        private void ApplyStonesFromLevelConfig(LevelConfig level, Board board)
        {
            if (level == null || board == null) return;
            if (level.stonePositions == null || level.stonePositions.Count == 0) return;

            for (int i = 0; i < level.stonePositions.Count; i++)
            {
                Vector2Int p = level.stonePositions[i];

                // Bounds guard
                if (p.x < 0 || p.x >= board.Width || p.y < 0 || p.y >= board.Height)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[GameBootstrapper] Stone position out of bounds: {p}");
#endif

                    continue;
                }

                var cell = board.Cells[p.x, p.y];

                cell.HasStone = true;
                cell.StoneHP = 2;             // MVP: always 2 hits
                cell.Tile = TileType.Empty;   // IMPORTANT: stone cell cannot hold a tile
            }
        }

        // UI butonları burayı çağırabilir (WinLosePresenter üzerinden)
        public void RestartLevel()
        {
            LoadCurrentLevel();
        }

        public void NextLevelOrRestartIfFinal()
        {
            if (levelManager != null && levelManager.TryAdvance())
            {
                LoadCurrentLevel(); // next level
            }
            else
            {
                // final level -> back to Level 1
                levelManager.ResetToFirst();
                LoadCurrentLevel();
            }
        }
    }
}

