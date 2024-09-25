namespace Dev.Infrastructure
{
    public interface IState
    {
        void Enter();
        void Exit();
    }

    public interface ITickState : IState
    {
        void Tick();
    }

    public interface IFixedNetworkTickState : ITickState
    {
        void FixedNetworkTick();
    }
}