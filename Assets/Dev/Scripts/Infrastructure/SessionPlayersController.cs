using Dev.BotsLogic;
using Dev.PlayerLogic;
using Fusion;

namespace Dev.Infrastructure
{
    public class SessionPlayersController
    {
        private PlayersDataService _playersDataService;
        private SessionStateService _sessionStateService;
        private BotsController _botsController;

        public SessionPlayersController(PlayersDataService playersDataService, SessionStateService sessionStateService, BotsController botsController)
        {
            _botsController = botsController;
            _sessionStateService = sessionStateService;
            _playersDataService = playersDataService;
        }
        
        
       
        
    }
}