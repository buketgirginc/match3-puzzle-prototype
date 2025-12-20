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
            _board.FillRandom();
            //debug için
            /*             var matches = _board.FindHorizontalMatches();
                        Debug.Log($"Horizontal matches: {matches.Count}");
                        foreach (var p in matches)
                            Debug.Log($"H match at {p}");
                        var vMatches = _board.FindVerticalMatches();
                        Debug.Log($"Vertical matches: {vMatches.Count}");
                        foreach (var p in vMatches)
                            Debug.Log($"V match at {p}"); */

            boardView.Init(_board);
        }
    }
}
