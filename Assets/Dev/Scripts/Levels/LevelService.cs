using Dev.Infrastructure;
using Fusion;
using UniRx;
using Zenject;

namespace Dev.Levels
{
    public class LevelService : NetworkContext
    {
        [Networked] public Level CurrentLevel { get; private set; }
        private LevelsContainer _levelsContainer;

        public static LevelService Instance { get; private set; }

        public Subject<Level> LevelLoaded { get; } = new Subject<Level>();

        private void Awake()
        {
            //_currentLevel = FindObjectOfType<Level>();

            if (Instance == null)
            {
                Instance = this;
            }
            
        }

        [Inject]
        private void Init(LevelsContainer levelsContainer)
        {
            _levelsContainer = levelsContainer;
        }

        public void LoadLevel(int lvl)
        {
            LevelStaticData levelStaticData = _levelsContainer.LevelStaticDatas[lvl - 1];

            var networkRunner = FindObjectOfType<NetworkRunner>();  

            Level level = networkRunner.Spawn(levelStaticData.Prefab);

            CurrentLevel = level;
            
            LevelLoaded.OnNext(level);
        }
        
    }
    
}