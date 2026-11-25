using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval Class Builder: Carrier (Fleet Hive Commander & Remote Strike Hull Class).
    /// Houses autonomous drone hangars, flight deck stance controls, and dedicated point-defense interception cannons.
    /// </summary>
    public static class CarrierClass
    {
        public static void ConfigureCarrier(ShipDefinition def)
        {
            def.id = "carrier";
            def.shipName = "Vanguard Carrier";
            def.shipClass = ShipClass.Carrier;
            def.tier = 3;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 1.30f;
            def.shieldHitboxOffset = 0.35f;
            def.canAimIndependently = true;
            def.turretTraverseSpeed = 105f;
            def.droneHangarCapacity = 4;
            def.defaultDroneType = "interceptor_drone";
            def.baseStats = new StatValues
            {
                shieldCapacity = 420f,
                shieldRegen = 25f,
                hullCapacity = 450f,
                energyCapacity = 220f,
                energyRegen = 22f,
                pulseDamage = 60f,
                pulseSpeed = 550f,
                speed = 160f,
                agility = 2.4f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Carrier_MainBattery", new Vector2(1.40f, 0f), SocketSize.Heavy, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Carrier_TurretL", new Vector2(0.20f, 1.45f), SocketSize.Medium, SocketMountType.Turret, 0f, "gatling"),
                new ShipSocketDefinition("Carrier_TurretR", new Vector2(0.20f, -1.45f), SocketSize.Medium, SocketMountType.Turret, 0f, "gatling"),
                new ShipSocketDefinition("Carrier_FlakAft", new Vector2(-1.60f, 0f), SocketSize.Hangar, SocketMountType.HangarBay, 0f, "scattergun")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Dorsal Command Turret", EquipmentSlotCategory.Weapon, new Vector2(1.40f, 0f), SocketSize.Heavy, SocketMountType.Turret, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_1", "Port PDC Gatling", EquipmentSlotCategory.Weapon, new Vector2(0.20f, 1.45f), SocketSize.Medium, SocketMountType.Turret, "gatling"),
                new ShipEquipmentSlotDefinition("wpn_2", "Starboard PDC Gatling", EquipmentSlotCategory.Weapon, new Vector2(0.20f, -1.45f), SocketSize.Medium, SocketMountType.Turret, "gatling"),
                new ShipEquipmentSlotDefinition("wpn_3", "Aft Drone Bay Cannon", EquipmentSlotCategory.Weapon, new Vector2(-1.60f, 0f), SocketSize.Hangar, SocketMountType.HangarBay, "scattergun"),
                new ShipEquipmentSlotDefinition("def_0", "Carrier Energy Shield", EquipmentSlotCategory.Defense, new Vector2(0.80f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "def_phase_deflector"),
                new ShipEquipmentSlotDefinition("def_1", "Heavy Armor Bulkhead", EquipmentSlotCategory.Defense, new Vector2(-0.80f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "def_nanite_armor"),
                new ShipEquipmentSlotDefinition("rea_0", "Fleet Capacitor Grid", EquipmentSlotCategory.Reactor, new Vector2(-1.20f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "rea_flux_capacitor"),
                new ShipEquipmentSlotDefinition("aux_0", "Automated Drone Foundry", EquipmentSlotCategory.Auxiliary, new Vector2(0f, 0.70f), SocketSize.Heavy, SocketMountType.FixedForward, "aux_escort_dock"),
                new ShipEquipmentSlotDefinition("aux_1", "Fleet Command Telemetry", EquipmentSlotCategory.Auxiliary, new Vector2(0f, -0.70f), SocketSize.Heavy, SocketMountType.FixedForward, "aux_targeting_comp")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-2.80f, 1.0f),
                new Vector2(-2.80f, -1.0f),
                new Vector2(-2.90f, 0f)
            };
        }

        public static void ConfigureVanguard(ShipDefinition def)
        {
            def.id = "vanguard";
            def.shipName = "Vanguard Carrier";
            def.shipClass = ShipClass.Carrier;
            def.tier = 3;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 0.80f;
            def.shieldHitboxOffset = 0.30f;
            def.canAimIndependently = true;
            def.turretTraverseSpeed = 130f;
            def.droneHangarCapacity = 4;
            def.defaultDroneType = "interceptor_drone";
            def.collisionArmorReduction = 0.25f;
            def.cameraOrthoSize = 13f;
            def.hullMass = 6f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 320f,
                shieldRegen = 18f,
                hullCapacity = 400f,
                energyCapacity = 220f,
                energyRegen = 25f,
                pulseDamage = 30f,
                pulseSpeed = 560f,
                speed = 160f,
                agility = 2.6f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Vanguard_Center", new Vector2(2.50f, 0f), SocketSize.Heavy, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Vanguard_PortProw", new Vector2(2.75f, 1.12f), SocketSize.Medium, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Vanguard_StarboardProw", new Vector2(2.75f, -1.12f), SocketSize.Medium, SocketMountType.Turret, 0f, "pulse")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-2.80f, 1.0f),
                new Vector2(-2.80f, -1.0f),
                new Vector2(-2.90f, 0f)
            };
        }
    }
}
