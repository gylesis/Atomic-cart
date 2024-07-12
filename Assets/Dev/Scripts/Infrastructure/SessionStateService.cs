using System.Linq;
using Dev.BotsLogic;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class SessionStateService : NetworkContext
    {
        private PlayersDataService _playersDataService;
        private BotsController _botsController;

        [Networked, Capacity(20)]
        public NetworkLinkedList<SessionPlayer> Players { get; }

        [Inject]
        private void Construct(PlayersDataService playersDataService, BotsController botsController)
        {
            _botsController = botsController;
            _playersDataService = playersDataService;
        }
        
        public SessionPlayer GetSessionPlayer(NetworkId id)
        {
            return Players.First(x => x.Id == id);
        }
        
        public SessionPlayer GetSessionPlayer(PlayerRef playerRef)
        {
            return GetSessionPlayer(playerRef.ToNetworkId());
        }
        
        public SessionPlayer GetSessionPlayer(Bot bot)
        {
            return Players.First(x => x.Id == bot.Object.Id);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AddPlayer(NetworkId id, string name, bool isBot, TeamSide teamSide)
        {
            SessionPlayer sessionPlayer = new SessionPlayer(id, name, isBot, teamSide, isBot ? PlayerRef.None : _playersDataService.GetPlayerBase(id).Object.InputAuthority);
            
            Players.Add(sessionPlayer);
            Debug.Log($"Session player added {name}. Count {Players.Count}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ChangePlayerId(PlayerRef playerRef, NetworkId newId)
        {
            SessionPlayer sessionPlayer = GetSessionPlayer(playerRef);
    
            SessionPlayer newSessionPlayer = new SessionPlayer(newId, sessionPlayer.Name, sessionPlayer.IsBot, sessionPlayer.TeamSide,
                sessionPlayer.Owner);

            SessionPlayer player = Players.First(x => x.Owner == playerRef);

            Players.Set(Players.IndexOf(player), newSessionPlayer);
        }   

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemovePlayer(NetworkId id)
        {
            bool hasPlayer = Players.Any(x => x.Id == id);

            if (hasPlayer)
            {
                SessionPlayer sessionPlayer = Players.First(x => x.Id == id);

                Players.Remove(sessionPlayer);
            }
        }

        public bool DoPlayerExist(PlayerRef playerRef)
        {
            return Players.Any(x => x.Owner == playerRef);
        }
        
        
        public void SetEnemiesFreezeState(bool toFreeze)
        {
            foreach (SessionPlayer sessionPlayer in Players)
            {
                NetworkId id = sessionPlayer.Id;
                
                if (sessionPlayer.IsBot)
                {
                    Bot bot = _botsController.GetBot(id);
                    
                    bot.SetFreezeState(toFreeze);
                }
                else
                {
                    PlayerBase player = _playersDataService.GetPlayerBase(id);
                    
                    player.PlayerController.SetAllowToMove(!toFreeze);
                    player.PlayerController.SetAllowToShoot(!toFreeze);
                }
            }

        }   

        public void RespawnAllPlayers()
        {
            foreach (SessionPlayer sessionPlayer in Players)
            {
                NetworkId id = sessionPlayer.Id;
                
                if (sessionPlayer.IsBot)
                {
                    Bot bot = _botsController.GetBot(id);
                    
                    _botsController.RPC_RespawnBot(bot);
                }
                else
                {
                    PlayerBase player = _playersDataService.GetPlayerBase(id);
                    
                    _playersDataService.PlayersSpawner.RespawnPlayerCharacter(player.Object.InputAuthority);
                }
            }
            
        }
    }
}