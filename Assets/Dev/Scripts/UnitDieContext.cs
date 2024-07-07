using Dev.Infrastructure;

namespace Dev
{
    public struct UnitDieContext
    {
        public SessionPlayer Killer;
        public SessionPlayer Victim;
        public bool IsKilledByServer;
    }
}