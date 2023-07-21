namespace Dev.Infrastructure
{
    public interface IState
    {
        void Enter();
        void Exit();
    }
}