using Dev.Infrastructure;
using Fusion;

namespace Dev.BotsLogic
{
    public class BotStateMachine : StateMachine<PatrolBotState>
    {
        private Bot _bot;
        private NetworkRunner _runner;

        public BotStateMachine(Bot bot, NetworkRunner runner)
        {
            _runner = runner;
            _bot = bot;
        }
        
    }
}