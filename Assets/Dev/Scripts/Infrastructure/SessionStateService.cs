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
        private TeamsService _teamsService;

        [Networked, Capacity(20)]
        public NetworkLinkedList<SessionPlayer> Players { get; }

        [Inject]
        private void Construct(PlayersDataService playersDataService, BotsController botsController, TeamsService teamsService)
        {
            _teamsService = teamsService;
            _botsController = botsController;
            _playersDataService = playersDataService;
        }

        public SessionPlayer GetSessionPlayer(NetworkId id)
        {
            return Players.FirstOrDefault(x => x.Id == id);
        }
        
        public SessionPlayer GetSessionPlayer(Bot bot)
        {
            return Players.FirstOrDefault(x => x.Id == bot.Object.Id);
        }

        public Result TryGetPlayerTeam(SessionPlayer player, out TeamSide teamSide)
        {
            teamSide = TeamSide.None;
            if (_teamsService.DoPlayerHasTeam(player) == false)
            {
                return Result.Error($"Player {player.Name}:{player.Id} has no team");
            }
            
            _teamsService.TryGetUnitTeamSide(player, out teamSide);
            return Result.Success();
        }
        
        public Result TryGetPlayerTeam(PlayerRef playerRef, out TeamSide teamSide)
        {
            teamSide = TeamSide.None;
            if (_teamsService.DoPlayerHasTeam(playerRef.ToNetworkId()) == false)
            {
                return Result.Error($"Player :{playerRef.PlayerId} has no team");
            }
            
            _teamsService.TryGetUnitTeamSide(playerRef, out teamSide);
            return Result.Success();
        }
        
        public Result TryGetPlayerTeam(NetworkId playerRef, out TeamSide teamSide)
        {
            teamSide = TeamSide.None;
            if (_teamsService.DoPlayerHasTeam(playerRef) == false)
            {
                return Result.Error($"Player {playerRef} has no team");
            }
            
            _teamsService.TryGetUnitTeamSide(playerRef, out teamSide);
            return Result.Success();
        }
        
        public bool TryGetBot(SessionPlayer sessionPlayer, out Bot bot)
        {
            bot = null;
            if (!sessionPlayer.IsBot) return false;
            bot = _botsController.GetBot(sessionPlayer.Id);
            return bot != null;
        }

        public void AddPlayer(NetworkId id, string name, bool isBot)
        {
            SessionPlayer sessionPlayer = new SessionPlayer(id, name, isBot, isBot ? PlayerRef.None : _playersDataService.GetPlayerBase(id).Object.InputAuthority);
            
            Players.Add(sessionPlayer);
            Debug.Log($"[RPC] Session player added {name}. Count {Players.Count}");
        }
        
        public void RemovePlayer(NetworkId id)
        {
            RPC_RemovePlayer(id);
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
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddPlayer(NetworkId id, string name, bool isBot)
        {
            SessionPlayer sessionPlayer = new SessionPlayer(id, name, isBot, isBot ? PlayerRef.None : _playersDataService.GetPlayerBase(id).Object.InputAuthority);
            
            Players.Add(sessionPlayer);
            Debug.Log($"[RPC] Session player added {name}. Count {Players.Count}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RemovePlayer(NetworkId id)
        {
            bool hasPlayer = Players.Any(x => x.Id == id);

            if (hasPlayer)
            {
                SessionPlayer sessionPlayer = Players.FirstOrDefault(x => x.Id == id);

                Players.Remove(sessionPlayer);
            }
        }
        
    }
}