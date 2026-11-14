namespace StarStrike.Core
{
    /// <summary>
    /// Contract for entities that store and consume energy (ships, shields, weapons).
    /// </summary>
    public interface IEnergyConsumer
    {
        float CurrentEnergy { get; }
        float MaxEnergy { get; }
        bool HasEnergy(float amount);
        bool ConsumeEnergy(float amount);
    }

    /// <summary>
    /// Contract for weapon systems capable of firing and managing cooldowns.
    /// </summary>
    public interface IWeapon
    {
        float FireRate { get; set; }
        float EnergyCost { get; }
        int WeaponSlots { get; set; }
        bool TryFire(IEnergyConsumer energySource = null);
        void ResetCooldown();
    }
}
