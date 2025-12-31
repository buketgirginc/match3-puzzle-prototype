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
                Debug.LogError("[GameBootstrapper] No LevelConfig found.");
                return;
            }

            // 1) model board
            _board = new Board();
            _board.Initialize(level.width, level.height);
            _board.FillRandomNoMatches();

            // 2) view
            boardView.Init(_board);

            // 3) state
            gameState.Init(level);
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

