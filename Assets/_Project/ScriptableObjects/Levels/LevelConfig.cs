using System.Collections.Generic;
using UnityEngine;
using Match3.Core;

namespace Match3.Levels
{
    [CreateAssetMenu(menuName = "Match3/Level Config", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("ID")]
        public int levelNumber = 1;

        [Header("Board")]
        public int width = 8;
        public int height = 8;

        [Header("Moves")]
        public int startingMoves = 20;

        [Header("Objectives")]
        public List<MatchObjectiveData> objectives = new();

        [Header("Reserved for later (Stone)")]
        public List<Vector2Int> stonePositions = new(); // şimdilik boş kalabilir
    }

    [System.Serializable]
    public struct MatchObjectiveData
    {
        public TileType type;
        public int target;
    }
}
