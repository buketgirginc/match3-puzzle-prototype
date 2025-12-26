using Match3.Core;

namespace Match3.Gameplay
{
    [System.Serializable]
    public class MatchObjective
    {
        public TileType type;
        public int target;
        public int current; // topladığın
    }
}
