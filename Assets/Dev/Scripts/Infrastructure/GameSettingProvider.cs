using Dev.Utils;

namespace Dev.Infrastructure
{
    public class GameSettingProvider
    {
        public static GameSettings GameSettings;

        public GameSettingProvider(GameSettings gameSettings)
        {
            GameSettings = gameSettings;
        }
    }
}