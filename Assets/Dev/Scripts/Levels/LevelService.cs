using System.Linq;
using Dev.Infrastructure;
using Dev.UI.PopUpsAndMenus;
using Fusion;
using UniRx;
using Zenject;

namespace Dev.Levels
{
    public class LevelService : NetSingleton<LevelService>
    {
        private MapsContainer _mapsContainer;
        [Networked] public Level CurrentLevel { get; private set; }

        public Subject<Level> LevelLoaded { get; } = new Subject<Level>();

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
                o.GetComponent<Level>().LevelName = levelName;
                DiInjecter.Instance.InjectGameObject(o.gameObject);
            });
            
            level.Object.ReleaseStateAuthority();

            CurrentLevel = level;
            
            LevelLoaded.OnNext(level);
        }
        
    }
    
}