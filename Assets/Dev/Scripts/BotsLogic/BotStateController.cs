﻿using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using Zenject;

namespace Dev.BotsLogic
{
    public class BotStateController : ITickable, IInitializable
    {
        private Bot _bot;
        private StateMachine<IBotState> _stateMachine;
        private GameSettings _gameSettings;
        private NetworkRunner _networkRunner;
        private TeamsService _teamsService;

        public GameSettings GameSettings => _gameSettings;
        public NetworkRunner NetworkRunner => _networkRunner;
        public TeamsService TeamsService => _teamsService;

        public bool HasStateAuthority => _bot?.HasStateAuthority ?? false;
        
        public StateMachine<IBotState> StateMachine => _stateMachine;
        
        
        public BotStateController(Bot bot, IBotState[] botStates, GameSettings gameSettings, NetworkRunner networkRunner, TeamsService teamsService)
        {
            _teamsService = teamsService;
            _networkRunner = networkRunner;
            _gameSettings = gameSettings;
            _bot = bot;
            _stateMachine = new StateMachine<IBotState>(botStates);
        }

        public void Initialize()
        {
            /*Transition transition = new Transition(typeof(AttackPlayerBotState), (() =>
            {
                
                
                return false;
            } ), int.MaxValue);
            
            _stateMachine.AddTransition(transition);*/
            
            _stateMachine.ChangeState<PatrolBotState>();
            
            _bot.SetRandomMovePos();
        }

        public void FixedNetworkTick()
        {
            _stateMachine.FixedNetworkTick();
        }

        public void Tick()
        {
            _stateMachine.Tick();
        }

    }
}