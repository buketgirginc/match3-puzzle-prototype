using UnityEngine;
using UnityEngine.SceneManagement;
using Match3.Gameplay;

public class WinLosePresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameState gameState;
    [SerializeField] private WinLosePanelView view;

    [Header("Temp (until LevelConfig)")]
    [SerializeField] private int currentLevelNumber = 1;
    [SerializeField] private bool isFinalLevel = false;

    private void Awake()
    {
        if (view == null)
            view = GetComponent<WinLosePanelView>();

        if (gameState == null)
            gameState = FindFirstObjectByType<GameState>(FindObjectsInactive.Exclude);
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

        view.Show(currentLevelNumber, win, isFinalLevel);


        view.SetButtonAction(RestartScene);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
