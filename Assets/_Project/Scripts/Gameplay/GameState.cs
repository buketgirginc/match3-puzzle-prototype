using System.Collections.Generic;
using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class GameState : MonoBehaviour
    {
        [Header("Moves")]
        [SerializeField] private int startingMoves = 20;
        public int MovesLeft { get; private set; }

        [Header("Objectives")]
        [SerializeField] private List<MatchObjective> objectives = new();

        public IReadOnlyList<MatchObjective> Objectives => objectives;

        public bool IsWin => objectives.TrueForAll(o => o.current >= o.target); //TrueForAll listedeki her elemana şunu uygular: 
                                                                                // “Her objective için o objective’in current’ı target’a eşit veya büyük mü?”
        public bool IsLose => MovesLeft <= 0 && !IsWin;
        public event System.Action<int> ObjectiveProgressChanged;
        public event System.Action ObjectivesReset;
        public event System.Action<int> MovesChanged;


        public void Init()
        {
            MovesLeft = startingMoves;
            foreach (var o in objectives) o.current = 0;

            ObjectivesReset?.Invoke();
            MovesChanged?.Invoke(MovesLeft);
        }

        public bool CanSpendMove()
        {
            return MovesLeft > 0 && !IsWin;
        }

        public void SpendMove()
        {
            int before = MovesLeft;
            MovesLeft = Mathf.Max(0, MovesLeft - 1); //hamle sayısını 1 azalt ama asla 0ın altına düşürme
            if (MovesLeft != before)
                MovesChanged?.Invoke(MovesLeft);
        }

        public void CollectFromMatches(HashSet<Vector2Int> matches, Board board)
        {
            foreach (var p in matches)
            {
                TileType t = board.Cells[p.x, p.y].Tile;
                if (t == TileType.Empty) continue;

                for (int i = 0; i < objectives.Count; i++)
                {
                    if (objectives[i].type == t && objectives[i].current < objectives[i].target)
                    {
                        objectives[i].current++;
                        ObjectiveProgressChanged?.Invoke(i);
                        break;
                    }
                }
            }
        }

    }
}