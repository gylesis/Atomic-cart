namespace Dev.Infrastructure
{
    public class GameSettingsProvider
    {
        public static GameSettings GameSettings;

        public GameSettingsProvider(GameSettings gameSettings)
        {
            GameSettings = gameSettings;
        }
    }
}