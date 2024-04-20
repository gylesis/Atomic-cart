using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.BotsLogic
{
    public class BotsController : NetworkContext
    {
        [SerializeField] private Bot _botPrefab;
        
        private TeamsService _teamsService;

        private void Start()
        {
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
        }
        
        [Inject]
        private void Construct(TeamsService teamsService)
        {
            _teamsService = teamsService;
        }
        
        private void OnLevelLoaded(Level level) 
        {
            int blueBots = 2;
            int redBots = 2;

            if (HasStateAuthority)
            {
                SpawnBots(blueBots, TeamSide.Blue);
                SpawnBots(redBots, TeamSide.Red);
            }   
        }
        
        private void SpawnBots(int botsCount, TeamSide botsSide)
        {
            for (int i = 0; i < botsCount; i++)
            {
                Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(botsSide);

                Runner.Spawn(_botPrefab, spawnPos, onBeforeSpawned: (runner, o) =>
                {
                    var bot = o.GetComponent<Bot>();

                    var botData = new BotData();
                    botData.TeamSide = botsSide;
                
                    bot.Init(botData);
                });
                
            }
        }
    }
}