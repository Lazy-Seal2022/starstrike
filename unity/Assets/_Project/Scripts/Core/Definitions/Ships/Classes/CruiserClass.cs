using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval Class Builder: Cruiser (Fleet Backbone & Heavy Warship Hull Class).
    /// Features 360-degree independent turret firing arcs, heavy energy shields, and broadside sponsor mounts.
    /// </summary>
    public static class CruiserClass
    {
        public static void ConfigureCruiser(ShipDefinition def)
        {
            def.id = "cruiser";
            def.shipName = "Aegis Cruiser";
            def.shipClass = ShipClass.Cruiser;
            def.tier = 3;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 1.10f;
            def.shieldHitboxOffset = 0.30f;
            def.canAimIndependently = true;
            def.turretTraverseSpeed = 120f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 350f,
                shieldRegen = 22f,
                hullCapacity = 320f,
                energyCapacity = 180f,
                energyRegen = 24f,
                pulseDamage = 55f,
                pulseSpeed = 580f,
                speed = 190f,
                agility = 3.2f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Cruiser_DorsalTurret", new Vector2(1.20f, 0f), SocketSize.Heavy, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Cruiser_VentralTurret", new Vector2(-0.30f, 0f), SocketSize.Heavy, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Cruiser_PortBroadside", new Vector2(0.40f, 1.40f), SocketSize.Medium, SocketMountType.BroadsideLeft, -90f, "scattergun"),
                new ShipSocketDefinition("Cruiser_StarboardBroadside", new Vector2(0.40f, -1.40f), SocketSize.Medium, SocketMountType.BroadsideRight, 90f, "scattergun"),
                new ShipSocketDefinition("Cruiser_AftPDC", new Vector2(-1.40f, 0f), SocketSize.Light, SocketMountType.Turret, 180f, "gatling")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Dorsal Heavy Turret Mount", EquipmentSlotCategory.Weapon, new Vector2(1.20f, 0f), SocketSize.Heavy, SocketMountType.Turret, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_1", "Ventral Heavy Turret Mount", EquipmentSlotCategory.Weapon, new Vector2(-0.30f, 0f), SocketSize.Heavy, SocketMountType.Turret, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_2", "Port Sponson Scattergun", EquipmentSlotCategory.Weapon, new Vector2(0.40f, 1.40f), SocketSize.Medium, SocketMountType.BroadsideLeft, "scattergun"),
                new ShipEquipmentSlotDefinition("wpn_3", "Starboard Sponson Scattergun", EquipmentSlotCategory.Weapon, new Vector2(0.40f, -1.40f), SocketSize.Medium, SocketMountType.BroadsideRight, "scattergun"),
                new ShipEquipmentSlotDefinition("wpn_4", "Aft Point Defense Turret", EquipmentSlotCategory.Weapon, new Vector2(-1.40f, 0f), SocketSize.Light, SocketMountType.Turret, "gatling"),
                new ShipEquipmentSlotDefinition("def_0", "Phase Deflector Shielding", EquipmentSlotCategory.Defense, new Vector2(0.50f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "def_phase_deflector"),
                new ShipEquipmentSlotDefinition("eng_0", "High-Output Sublight Thrusters", EquipmentSlotCategory.Engine, new Vector2(-1.80f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "eng_afterburner"),
                new ShipEquipmentSlotDefinition("rea_0", "Cruiser Antimatter Drive", EquipmentSlotCategory.Reactor, new Vector2(-0.90f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "rea_tachyon_core"),
                new ShipEquipmentSlotDefinition("aux_0", "Fire Control Targeting Grid", EquipmentSlotCategory.Auxiliary, new Vector2(1.80f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "aux_targeting_comp")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-2.40f, 0.80f),
                new Vector2(-2.40f, -0.80f),
                new Vector2(-2.50f, 0f)
            };
        }
    }
}
