using System.Linq;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersScoreUI : NetworkContext
    {
        [SerializeField] private PlayerScoreUIContainer _blueTeamScores;
        [SerializeField] private PlayerScoreUIContainer _redTeamScores;

        [SerializeField] private PlayerScoreUI _playerScoreUIPrefab;
        private TeamsService _teamsService;
        private PlayersSpawner _playersSpawner;

        [Networked, Capacity(20)] private NetworkLinkedList<PlayerScoreUI> ScoreUis { get; }

        [Inject]
        private void Init(TeamsService teamsService, PlayersSpawner playersSpawner)
        {
            _teamsService = teamsService;
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            if (HasStateAuthority == false)
            {
                return;
            }

            _playersSpawner.PlayerDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            for (var index = ScoreUis.Count - 1; index >= 0; index--)
            {
                PlayerScoreUI playerScoreUI = ScoreUis.Get(index);

                if (playerScoreUI.PlayerId == playerRef)
                {
                    ScoreUis.Remove(playerScoreUI);
                    RPC_Destroy(playerScoreUI);
                }
            }
        }

        [Rpc]
        private void RPC_Destroy(PlayerScoreUI playerScoreUI)
        {
            Destroy(playerScoreUI.gameObject);
        }


        public void UpdateScores(PlayerScoreData[] scoreDatas)
        {
            for (var index = 0; index < scoreDatas.Length; index++)
            {
                PlayerScoreData scoreData = scoreDatas[index];
                PlayerRef playerId = scoreData.PlayerId;

                var hasScoreUI = ScoreUis.FirstOrDefault(x => x.PlayerId == playerId) != null;
                
                if (hasScoreUI)
                {
                    PlayerScoreUI playerScoreUI = ScoreUis.Get(index);

                    playerScoreUI.RPC_UpdateData(scoreData.PlayerFragCount, scoreData.PlayerDeathCount);
                    playerScoreUI.RPC_InitNickname(scoreData.Nickname.Value);

                    continue;
                }

                PlayerScoreUI scoreUI = Runner.Spawn(_playerScoreUIPrefab, Vector3.zero, Quaternion.identity,
                    scoreData.PlayerId,
                    (runner, o) =>
                    {
                        PlayerScoreUI playerScoreUI = o.GetComponent<PlayerScoreUI>();
                        playerScoreUI.RPC_Init(scoreData.Nickname.Value, scoreData.PlayerFragCount,
                            scoreData.PlayerDeathCount,
                            playerId);
                    });

                ScoreUis.Add(scoreUI);
            }

            RPC_ApplyParents();
        }

        [Rpc]
        private void RPC_ApplyParents()
        {
            foreach (PlayerScoreUI playerScoreUI in ScoreUis)
            {
                TeamSide playerTeamSide = _teamsService.GetUnitTeamSide(playerScoreUI.PlayerId);

                RPC_SetScoreUIParent(playerScoreUI, playerTeamSide);
            }
        }


        [Rpc]
        private void RPC_SetScoreUIParent(PlayerScoreUI playerScoreUI, TeamSide teamSide)
        {
            if (teamSide == TeamSide.Blue)
            {
                playerScoreUI.transform.parent = _blueTeamScores.ScoresUIParent;
            }
            else
            {
                playerScoreUI.transform.parent = _redTeamScores.ScoresUIParent;
            }
        }
    }
}