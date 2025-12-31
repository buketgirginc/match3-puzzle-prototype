using UnityEngine;

namespace Match3.Levels
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Levels (order matters)")]
        [SerializeField] private LevelConfig[] levels;

        public int CurrentIndex { get; private set; }

        public LevelConfig CurrentLevel =>
            (levels != null && levels.Length > 0 && CurrentIndex >= 0 && CurrentIndex < levels.Length)
                ? levels[CurrentIndex]
                : null;

        public bool IsFinalLevel =>
            levels == null || levels.Length == 0 || CurrentIndex >= levels.Length - 1;

        public void StartAt(int index)
        {
            CurrentIndex = Mathf.Clamp(index, 0, (levels?.Length ?? 1) - 1);
        }

        public bool TryAdvance()
        {
            if (IsFinalLevel) return false;
            CurrentIndex++;
            return true;
        }

        public void RestartCurrent()
        {
            // index değişmez; bootstrapper reload yapacak
        }
        public void ResetToFirst()
        {
            CurrentIndex = 0;
        }
    }
}
