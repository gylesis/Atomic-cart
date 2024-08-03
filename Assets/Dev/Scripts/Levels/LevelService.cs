using System.Linq;
using Dev.Infrastructure;
using Dev.UI.PopUpsAndMenus;
using Fusion;
using UniRx;
using Zenject;

namespace Dev.Levels
{
    public class LevelService : NetworkContext
    {
        private MapsContainer _mapsContainer;
        [Networked] public Level CurrentLevel { get; private set; }

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
        private void Init(MapsContainer mapsContainer)
        {
            _mapsContainer = mapsContainer;
        }

        public void LoadLevel(string levelName)
        {
            MapData levelStaticData = _mapsContainer.MapDatas.First(x =>x.Name == levelName);
    
            Level level = Runner.Spawn(levelStaticData.Prefab, onBeforeSpawned: (runner, o) =>
            {
                DiInjecter.Instance.InjectGameObject(o.gameObject);
            });
            
            level.Object.ReleaseStateAuthority();

            CurrentLevel = level;
            
            LevelLoaded.OnNext(level);
        }
        
    }
    
}