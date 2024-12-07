namespace Dev.BotsLogic.GOAP
{
    public abstract class BotAction<TPayload> : IBotAction
    {
        protected Bot Bot;
        public TPayload Data { get; set; }
        public abstract float Cost { get; }

        protected BotAction(TPayload data, Bot bot)
        {
            Bot = bot;
            Data = data;
        }
        
        public abstract void Execute();
        public abstract bool CanExecute();
    }
}