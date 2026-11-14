namespace StarStrike.Core
{
    /// <summary>
    /// Contract for projectiles traveling through the simulation.
    /// </summary>
    public interface IProjectile
    {
        float Damage { get; }
        float Speed { get; }
        bool IsEnemy { get; }
    }
}
