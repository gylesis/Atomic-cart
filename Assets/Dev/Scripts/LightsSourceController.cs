using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.Lobby;
using Dev.Levels;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class LightsSourceController : MonoBehaviour
    {
        private MapsContainer _mapsContainer;
        private BotsController _botsController;
        private PlayersSpawner _playersSpawner;
        private LevelService _levelService;

        [Inject]
        private void Construct(MapsContainer mapsContainer, PlayersSpawner playersSpawner, BotsController botsController, LevelService levelService)
        {
            _levelService = levelService;
            _playersSpawner = playersSpawner;
            _botsController = botsController;
            _mapsContainer = mapsContainer;
        }

        private async void Start()
        {
            var levelService = await LevelService.WaitForNetInitialization();

            if (levelService.CurrentLevel != null) 
                OnLevelLoaded(levelService.CurrentLevel);

            levelService.LevelLoaded.Subscribe(OnLevelLoaded).AddTo(GlobalDisposable.SceneScopeToken);

            _botsController.BotSpawned.Subscribe(OnBotSpawned).AddTo(GlobalDisposable.SceneScopeToken);
            _playersSpawner.CharacterSpawned.Subscribe(OnPlayerSpawned).AddTo(GlobalDisposable.SceneScopeToken);
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext context)
        {
            var lightSources = context.Transform.GetComponentsInChildren<LightSource>().ToList();
            UpdateSources(lightSources);
        }

        private void OnBotSpawned(Bot bot)
        {
            var lightSources = bot.GetComponentsInChildren<LightSource>().ToList();
            UpdateSources(lightSources);
        }

        private void UpdateSources(List<LightSource> lightSources)
        {
            var mapData = _mapsContainer.GetMapData(_levelService.CurrentLevel.LevelName);
            
            foreach (var lightSource in lightSources) 
                lightSource.Light2D.enabled = mapData.SupportLighting;
        }

        private void OnLevelLoaded(Level level)
        {
            List<LightSource> lightSources = new List<LightSource>();
            
            lightSources.AddRange(level.LightSources);

            foreach (var player in _playersSpawner.PlayersBases) 
                lightSources.AddRange(player.GetComponentsInChildren<LightSource>());
            
            foreach (var bot in _botsController.AliveBots) 
                lightSources.AddRange(bot.GetComponentsInChildren<LightSource>());
            
            UpdateSources(lightSources);
        }
    }
}