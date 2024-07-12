using System;
using System.Linq;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersDataService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;
        private SessionStateService _sessionStateService;
        private HealthObjectsService _healthObjectsService;

        public PlayersSpawner PlayersSpawner => _playersSpawner;

        [Inject]
        public void Init(SessionStateService sessionStateService, PlayersSpawner playersSpawner, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _playersSpawner = playersSpawner;
            _sessionStateService = sessionStateService;
        }
        
        public string GetNickname(PlayerRef playerRef)
        {
            return _sessionStateService.GetSessionPlayer(playerRef).Name;
        }
        
        public PlayerCharacter GetPlayer(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayer(playerRef);
        }

        public NetworkId GetPlayerCharacterId(PlayerRef playerRef)
        {
            return GetPlayer(playerRef).Object.Id;
        }
        
        public PlayerBase GetPlayerBase(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayerBase(playerRef);
        }
        
        public PlayerBase GetPlayerBase(NetworkId id)
        {
            return _playersSpawner.GetPlayerBase(id);
        }
        
        public CharacterClass GetPlayerCharacterClass(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayerBase(playerRef).CharacterClass;
        }
        
        public Vector3 GetPlayerPos(PlayerRef playerRef) => GetPlayer(playerRef).transform.position;


        private void OnGUI()
        {
            if(Object == null) return;

            float height = 25;
            float width = 250;
            float heightStepPerPlayer = 50;

           // int playersCount = _sessionStateService.Players.Count(x => x.IsBot == false);
    
            //Vector2 center = new Vector2(25, (playersCount * heightStepPerPlayer) / 2 - height);
            
            //GUI.Box(new Rect(center.x, center.y, width, (playersCount * heightStepPerPlayer)), GUIContent.none);
                
            foreach (SessionPlayer player in _sessionStateService.Players)
            {
                if(player.IsBot) continue;

                PlayerBase playerBase = GetPlayerBase(player.Owner);
                
                if(playerBase.Character == null) continue;
                
                NetworkId id = playerBase.Character.Object.Id;
                int health = _healthObjectsService.GetHealth(id);
                string playerName = player.Name;

                Rect rect = new Rect(25, height, width, heightStepPerPlayer + 5);
                string text = $"{playerName} : {health}";
                GUIStyle guiStyle = new GUIStyle();
                guiStyle.fontSize = 35;
                guiStyle.normal.textColor = Color.white;

                GUI.Label(rect, text, guiStyle);
                
                height += heightStepPerPlayer;
            }
        }
    }
    
}