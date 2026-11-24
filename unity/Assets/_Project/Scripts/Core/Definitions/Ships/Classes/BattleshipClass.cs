using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval Class Builder: Battleship (Superheavy Dreadnought & Capital Citadel Hull Class).
    /// Boasts colossal spinal superweapons, heavy armor damage reduction, and multiple independent turret batteries.
    /// </summary>
    public static class BattleshipClass
    {
        public static void ConfigureBattleship(ShipDefinition def)
        {
            def.id = "battleship";
            def.shipName = "Titan Dreadnought";
            def.shipClass = ShipClass.Battleship;
            def.tier = 3;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 1.55f;
            def.shieldHitboxOffset = 0.40f;
            def.canAimIndependently = true;
            def.turretTraverseSpeed = 95f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 500f,
                shieldRegen = 28f,
                hullCapacity = 600f,
                energyCapacity = 300f,
                energyRegen = 20f,
                pulseDamage = 75f,
                pulseSpeed = 520f,
                speed = 130f,
                agility = 1.8f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Battleship_SpinalLance", new Vector2(3.10f, 0f), SocketSize.Spinal, SocketMountType.Spinal, 0f, "beam"),
                new ShipSocketDefinition("Battleship_HeavyTurretL", new Vector2(1.70f, 1.25f), SocketSize.Heavy, SocketMountType.Turret, 0f, "scattergun"),
                new ShipSocketDefinition("Battleship_HeavyTurretR", new Vector2(1.70f, -1.25f), SocketSize.Heavy, SocketMountType.Turret, 0f, "scattergun"),
                new ShipSocketDefinition("Battleship_BridgeTurret", new Vector2(0.40f, 0f), SocketSize.Heavy, SocketMountType.Turret, 0f, "gatling"),
                new ShipSocketDefinition("Battleship_MissilePodL", new Vector2(-0.80f, 1.70f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "missile"),
                new ShipSocketDefinition("Battleship_MissilePodR", new Vector2(-0.80f, -1.70f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "missile")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Spinal Lance Superweapon", EquipmentSlotCategory.Weapon, new Vector2(3.10f, 0f), SocketSize.Spinal, SocketMountType.Spinal, "beam"),
                new ShipEquipmentSlotDefinition("wpn_1", "Port Heavy Flak Turret", EquipmentSlotCategory.Weapon, new Vector2(1.70f, 1.25f), SocketSize.Heavy, SocketMountType.Turret, "scattergun"),
                new ShipEquipmentSlotDefinition("wpn_2", "Starboard Heavy Flak Turret", EquipmentSlotCategory.Weapon, new Vector2(1.70f, -1.25f), SocketSize.Heavy, SocketMountType.Turret, "scattergun"),
                new ShipEquipmentSlotDefinition("wpn_3", "Bridge Command Gatling", EquipmentSlotCategory.Weapon, new Vector2(0.40f, 0f), SocketSize.Heavy, SocketMountType.Turret, "gatling"),
                new ShipEquipmentSlotDefinition("wpn_4", "Port Heavy Missile Pod", EquipmentSlotCategory.Weapon, new Vector2(-0.80f, 1.70f), SocketSize.Heavy, SocketMountType.FixedForward, "missile"),
                new ShipEquipmentSlotDefinition("wpn_5", "Starboard Heavy Missile Pod", EquipmentSlotCategory.Weapon, new Vector2(-0.80f, -1.70f), SocketSize.Heavy, SocketMountType.FixedForward, "missile"),
                new ShipEquipmentSlotDefinition("def_0", "Phase Fortress Aegis", EquipmentSlotCategory.Defense, new Vector2(1.0f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "def_phase_deflector"),
                new ShipEquipmentSlotDefinition("def_1", "Dreadnought Citadel Armor", EquipmentSlotCategory.Defense, new Vector2(-0.2f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "def_nanite_armor"),
                new ShipEquipmentSlotDefinition("rea_0", "Primary Singularity Core", EquipmentSlotCategory.Reactor, new Vector2(-1.5f, 0.6f), SocketSize.Heavy, SocketMountType.FixedForward, "rea_flux_capacitor"),
                new ShipEquipmentSlotDefinition("rea_1", "Secondary Tachyon Loop", EquipmentSlotCategory.Reactor, new Vector2(-1.5f, -0.6f), SocketSize.Heavy, SocketMountType.FixedForward, "rea_tachyon_core"),
                new ShipEquipmentSlotDefinition("aux_0", "Supercapital Targeting Mainframe", EquipmentSlotCategory.Auxiliary, new Vector2(-2.2f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "aux_targeting_comp")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-3.40f, 1.20f),
                new Vector2(-3.40f, -1.20f),
                new Vector2(-3.60f, 0.40f),
                new Vector2(-3.60f, -0.40f)
            };
        }
    }
}
