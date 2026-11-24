using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval Class Builder: Fighter (Light Strike & Interceptor Hull Class).
    /// Encompasses high-speed dogfighters, recon scouts, and stealth interceptors.
    /// </summary>
    public static class FighterClass
    {
        public static void ConfigureFighter(ShipDefinition def)
        {
            def.id = "fighter";
            def.shipName = "Ares Fighter";
            def.shipClass = ShipClass.Fighter;
            def.tier = 1;
            def.maxUpgradeLevel = 5;
            def.hitboxRadius = 0.50f;
            def.shieldHitboxOffset = 0.18f;
            def.turretTraverseSpeed = 280f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 120f,
                shieldRegen = 14f,
                hullCapacity = 70f,
                energyCapacity = 80f,
                energyRegen = 26f,
                pulseDamage = 26f,
                pulseSpeed = 680f,
                speed = 340f,
                agility = 7.5f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Fighter_WingL", new Vector2(0.40f, 0.85f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse"),
                new ShipSocketDefinition("Fighter_WingR", new Vector2(0.40f, -0.85f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Port Strike Mount", EquipmentSlotCategory.Weapon, new Vector2(0.40f, 0.85f), SocketSize.Light, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_1", "Starboard Strike Mount", EquipmentSlotCategory.Weapon, new Vector2(0.40f, -0.85f), SocketSize.Light, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("eng_0", "High-Flow Thrusters", EquipmentSlotCategory.Engine, new Vector2(-1.20f, 0f), SocketSize.Light, SocketMountType.FixedForward, "eng_afterburner"),
                new ShipEquipmentSlotDefinition("rea_0", "Micro Reactor Core", EquipmentSlotCategory.Reactor, new Vector2(-0.40f, 0f), SocketSize.Light, SocketMountType.FixedForward, "rea_flux_capacitor")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-1.20f, 0.35f),
                new Vector2(-1.20f, -0.35f)
            };
        }

        public static void ConfigureScout(ShipDefinition def)
        {
            def.id = "scout";
            def.shipName = "Delta Scout";
            def.shipClass = ShipClass.Fighter;
            def.tier = 1;
            def.maxUpgradeLevel = 4;
            def.hitboxRadius = 0.55f;
            def.shieldHitboxOffset = 0.18f;
            def.turretTraverseSpeed = 260f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 100f,
                shieldRegen = 8f,
                hullCapacity = 80f,
                energyCapacity = 80f,
                energyRegen = 18f,
                pulseDamage = 18f,
                pulseSpeed = 600f,
                speed = 260f,
                agility = 4.5f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Scout_Center", new Vector2(1.55f, 0f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-1.55f, 0f)
            };
        }
    }
}
