using Dev.Infrastructure;

namespace Dev.Levels
{
    public class LevelService : NetworkContext
    {
        private Level _currentLevel;

        public Level CurrentLevel => _currentLevel;

        public static LevelService Instance { get; private set; }

        private void Awake()
        {
            _currentLevel = FindObjectOfType<Level>();

            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}