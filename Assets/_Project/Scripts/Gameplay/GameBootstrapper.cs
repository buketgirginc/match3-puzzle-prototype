using Match3.Gameplay;
using UnityEngine;

namespace Match3
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;

        private Board _board;

        private void Start()
        {
            _board = new Board();
            _board.Initialize(8, 8);
            _board.FillRandomNoMatches();

            boardView.Init(_board);
        }
    }
}
