using UnityEngine;
using Match3.Gameplay;
using Match3.Levels;

public class WinLosePresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameState gameState;
    [SerializeField] private WinLosePanelView view;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Match3.GameBootstrapper bootstrapper;

    private void Awake()
    {
        if (view == null)
            view = GetComponent<WinLosePanelView>();

        if (gameState == null)
            gameState = FindFirstObjectByType<GameState>(FindObjectsInactive.Exclude);

        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>(FindObjectsInactive.Exclude);

        if (bootstrapper == null)
            bootstrapper = FindFirstObjectByType<Match3.GameBootstrapper>(FindObjectsInactive.Exclude);
    }

    private void OnEnable()
    {
        if (view != null)
            view.Hide();

        if (gameState != null)
            gameState.GameOver += OnGameOver;
        else
            Debug.LogWarning("WinLosePresenter: GameState reference missing.");
    }

    private void OnDisable()
    {
        if (gameState != null)
            gameState.GameOver -= OnGameOver;
    }

    private void OnGameOver(bool win)
    {
        if (view == null) return;

        int levelNumber = 1;
        bool isFinalLevel = false;

        if (levelManager != null && levelManager.CurrentLevel != null)
        {
            levelNumber = levelManager.CurrentLevel.levelNumber;
            isFinalLevel = levelManager.IsFinalLevel;
        }

        view.Show(levelNumber, win, isFinalLevel);

        if (bootstrapper == null)
        {
            Debug.LogWarning("WinLosePresenter: GameBootstrapper reference missing.");
            return;
        }

        if (!win)
        {
            // LOSE -> Restart
            view.SetButtonAction(() =>
            {
                view.Hide();
                bootstrapper.RestartLevel();
            });
        }
        else
        {
            // WIN -> Next level (if not final), else Restart
            view.SetButtonAction(() =>
            {
                view.Hide();
                bootstrapper.NextLevelOrRestartIfFinal();
            });
        }
    }
}
