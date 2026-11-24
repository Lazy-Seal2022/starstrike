namespace StarStrike.Core
{
    /// <summary>
    /// Contract for entities that store and consume energy (ships, shields, weapons).
    /// </summary>
    public interface IEnergyConsumer
    {
        float CurrentEnergy { get; }
        float MaxEnergy { get; }
        bool IsEnergyLockedOut { get; }
        bool HasEnergy(float amount);
        bool ConsumeEnergy(float amount);
    }

    public enum WeaponArchetype
    {
        Projectile,
        Traceable,
        Beam
    }

    /// <summary>
    /// Contract for weapon systems capable of firing and managing cooldowns.
    /// </summary>
    public interface IWeapon
    {
        string WeaponName { get; }
        WeaponArchetype Archetype { get; }
        float FireRate { get; set; }
        float EnergyCost { get; }
        int WeaponSlots { get; set; }
        float InaccuracyAngle { get; }
        bool TryFire(IEnergyConsumer energySource = null);
        void StopFiring();
        void ResetCooldown();
    }
}
