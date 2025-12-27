using UnityEngine;
using Match3.Gameplay;

public class MovesPresenter : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private MovesTextView view;

    private void Awake()
    {
        if (view == null)
            view = GetComponent<MovesTextView>();
    }

    private void OnEnable()
    {
        if (gameState == null || view == null) return;

        view.SetMoves(gameState.MovesLeft);
        gameState.MovesChanged += OnMovesChanged;
    }

    private void OnDisable()
    {
        if (gameState == null) return;
        gameState.MovesChanged -= OnMovesChanged;
    }

    private void OnMovesChanged(int movesLeft)
    {
        view.SetMoves(movesLeft);
    }
}
