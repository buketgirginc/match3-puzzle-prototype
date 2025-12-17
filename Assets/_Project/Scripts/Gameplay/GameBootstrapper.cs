using Match3.Gameplay;
using UnityEngine;

namespace Match3
{
    public class GameBootstrapper : MonoBehaviour
    {
        private Board _board;

        private void Start()
        {
            _board = new Board();
            _board.Initialize(8, 8);

            Debug.Log($"Board initialized: {_board.Width}x{_board.Height}");
            _board.FillRandom();
            Debug.Log(_board.DebugPrint());
        }
    }
}
