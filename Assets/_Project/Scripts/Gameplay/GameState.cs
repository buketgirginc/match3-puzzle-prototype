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

        public void Init()
        {
            MovesLeft = startingMoves;
            foreach (var o in objectives) o.current = 0;
        }

        public bool CanSpendMove()
        {
            return MovesLeft > 0 && !IsWin;
        }

        public void SpendMove() //hamle sayısını 1 azalt ama asla 0ın altına düşürme
        {
            MovesLeft = Mathf.Max(0, MovesLeft - 1);
        }

        public void CollectFromMatches(HashSet<Vector2Int> matches, Board board)
        {
            foreach (var p in matches)
            {
                TileType t = board.Cells[p.x, p.y].Tile;
                if (t == TileType.Empty) continue; //boşsa o pozisyonu atla

                for (int i = 0; i < objectives.Count; i++)
                {
                    if (objectives[i].type == t && objectives[i].current < objectives[i].target)
                    {
                        objectives[i].current++;
                        break;
                    }
                }
            }
        }
    }
}