namespace StarStrike.Core
{
    /// <summary>
    /// Contract for objects recycled via PoolManager, enabling clean lifecycle resets.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
