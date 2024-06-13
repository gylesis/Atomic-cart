namespace Dev.Weapons.Guns
{
    public interface IDamageable
    {
        DamagableType DamageId { get; }
    }   

    public enum DamagableType
    {
        ObstacleWithHealth = 0,
        Obstacle = 1,
        Bot = 2,
        Player = 3,
        DummyTarget = 4,
    }
}