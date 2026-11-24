using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval Class Builder: Frigate (Fast Wolfpack & Tactical Escort Hull Class).
    /// Balances multi-role forward armaments, utility modules, and tactical flank maneuverability.
    /// </summary>
    public static class FrigateClass
    {
        public static void ConfigureFrigate(ShipDefinition def)
        {
            def.id = "frigate";
            def.shipName = "Viper Frigate";
            def.shipClass = ShipClass.Frigate;
            def.tier = 2;
            def.maxUpgradeLevel = 5;
            def.hitboxRadius = 0.65f;
            def.shieldHitboxOffset = 0.22f;
            def.turretTraverseSpeed = 180f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 180f,
                shieldRegen = 16f,
                hullCapacity = 140f,
                energyCapacity = 110f,
                energyRegen = 22f,
                pulseDamage = 32f,
                pulseSpeed = 650f,
                speed = 280f,
                agility = 5.2f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Frigate_Nose", new Vector2(1.60f, 0f), SocketSize.Medium, SocketMountType.FixedForward, 0f, "pulse"),
                new ShipSocketDefinition("Frigate_FlankL", new Vector2(0.30f, 1.10f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse"),
                new ShipSocketDefinition("Frigate_FlankR", new Vector2(0.30f, -1.10f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Spinal Prow Mount", EquipmentSlotCategory.Weapon, new Vector2(1.60f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_1", "Port Interceptor Hardpoint", EquipmentSlotCategory.Weapon, new Vector2(0.30f, 1.10f), SocketSize.Light, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_2", "Starboard Interceptor Hardpoint", EquipmentSlotCategory.Weapon, new Vector2(0.30f, -1.10f), SocketSize.Light, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("def_0", "Nanite Hull Lattice", EquipmentSlotCategory.Defense, new Vector2(-0.40f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_nanite_armor"),
                new ShipEquipmentSlotDefinition("rea_0", "Compact Fusion Generator", EquipmentSlotCategory.Reactor, new Vector2(-0.80f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "rea_flux_capacitor"),
                new ShipEquipmentSlotDefinition("aux_0", "Targeting Computer v2", EquipmentSlotCategory.Auxiliary, new Vector2(0.60f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "aux_targeting_comp")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-1.50f, 0.40f),
                new Vector2(-1.50f, -0.40f)
            };
        }
    }
}
