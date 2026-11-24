using System.Collections.Generic;
using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Factory for generating and querying the 6 naval ship archetypes and baseline configurations.
    /// </summary>
    public static class ShipArchetypeFactory
    {
        public static ShipDefinition CreateArchetype(ShipClass shipClass)
        {
            var def = ScriptableObject.CreateInstance<ShipDefinition>();
            def.shipClass = shipClass;
            def.classRoleDescription = shipClass.GetClassRoleDescription();
            def.cameraOrthoSize = shipClass.GetDefaultCameraSize();
            def.hullMass = shipClass.GetDefaultHullMass();
            def.collisionArmorReduction = shipClass.GetCollisionArmorReduction();

            switch (shipClass)
            {
                case ShipClass.Fighter:
                    ConfigureFighter(def);
                    break;
                case ShipClass.Frigate:
                    ConfigureFrigate(def);
                    break;
                case ShipClass.Destroyer:
                    ConfigureDestroyer(def);
                    break;
                case ShipClass.Cruiser:
                    ConfigureCruiser(def);
                    break;
                case ShipClass.Carrier:
                    ConfigureCarrier(def);
                    break;
                case ShipClass.Battleship:
                    ConfigureBattleship(def);
                    break;
            }

            // Sync legacy weaponSockets for 100% backward compatibility
            def.weaponSockets = def.GetWeaponSocketPositions();
            def.weaponSlots = def.hardpointSockets != null ? def.hardpointSockets.Length : 1;
            return def;
        }

        public static List<ShipDefinition> CreateAllArchetypes()
        {
            var list = new List<ShipDefinition>();
            foreach (ShipClass sc in System.Enum.GetValues(typeof(ShipClass)))
            {
                list.Add(CreateArchetype(sc));
            }
            return list;
        }

        private static void ConfigureFighter(ShipDefinition def)
        {
            def.id = "fighter";
            def.shipName = "Ares Fighter";
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

        private static void ConfigureFrigate(ShipDefinition def)
        {
            def.id = "frigate";
            def.shipName = "Viper Frigate";
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
                pulseSpeed = 640f,
                speed = 280f,
                agility = 5.8f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Frigate_CenterProw", new Vector2(1.65f, 0f), SocketSize.Medium, SocketMountType.FixedForward, 0f, "beam"),
                new ShipSocketDefinition("Frigate_FlankL", new Vector2(0.50f, 1.15f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse"),
                new ShipSocketDefinition("Frigate_FlankR", new Vector2(0.50f, -1.15f), SocketSize.Light, SocketMountType.FixedForward, 0f, "pulse")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Center Prow Beam", EquipmentSlotCategory.Weapon, new Vector2(1.65f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "beam"),
                new ShipEquipmentSlotDefinition("wpn_1", "Port Flank Hardpoint", EquipmentSlotCategory.Weapon, new Vector2(0.50f, 1.15f), SocketSize.Light, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_2", "Starboard Flank Hardpoint", EquipmentSlotCategory.Weapon, new Vector2(0.50f, -1.15f), SocketSize.Light, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("def_0", "Shield Generator Bay", EquipmentSlotCategory.Defense, new Vector2(0f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_shield_booster"),
                new ShipEquipmentSlotDefinition("eng_0", "Vector Thrusters", EquipmentSlotCategory.Engine, new Vector2(-1.60f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "eng_vector_thrusters"),
                new ShipEquipmentSlotDefinition("aux_0", "Graviton Siphon", EquipmentSlotCategory.Auxiliary, new Vector2(-0.60f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "aux_graviton_siphon")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-1.60f, 0.45f),
                new Vector2(-1.60f, -0.45f)
            };
        }

        private static void ConfigureDestroyer(ShipDefinition def)
        {
            def.id = "destroyer";
            def.shipName = "Goliath Destroyer";
            def.tier = 2;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 0.80f;
            def.shieldHitboxOffset = 0.25f;
            def.turretTraverseSpeed = 150f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 240f,
                shieldRegen = 18f,
                hullCapacity = 220f,
                energyCapacity = 140f,
                energyRegen = 20f,
                pulseDamage = 45f,
                pulseSpeed = 580f,
                speed = 230f,
                agility = 4.2f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Destroyer_MainGunL", new Vector2(1.85f, 0.65f), SocketSize.Medium, SocketMountType.FixedForward, 0f, "scattergun"),
                new ShipSocketDefinition("Destroyer_MainGunR", new Vector2(1.85f, -0.65f), SocketSize.Medium, SocketMountType.FixedForward, 0f, "scattergun"),
                new ShipSocketDefinition("Destroyer_TorpedoL", new Vector2(0.60f, 1.45f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "missile"),
                new ShipSocketDefinition("Destroyer_TorpedoR", new Vector2(0.60f, -1.45f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "missile")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Port Main Gun", EquipmentSlotCategory.Weapon, new Vector2(1.85f, 0.65f), SocketSize.Medium, SocketMountType.FixedForward, "scattergun"),
                new ShipEquipmentSlotDefinition("wpn_1", "Starboard Main Gun", EquipmentSlotCategory.Weapon, new Vector2(1.85f, -0.65f), SocketSize.Medium, SocketMountType.FixedForward, "scattergun"),
                new ShipEquipmentSlotDefinition("wpn_2", "Port Heavy Torpedo Silo", EquipmentSlotCategory.Weapon, new Vector2(0.60f, 1.45f), SocketSize.Heavy, SocketMountType.FixedForward, "missile"),
                new ShipEquipmentSlotDefinition("wpn_3", "Starboard Heavy Torpedo Silo", EquipmentSlotCategory.Weapon, new Vector2(0.60f, -1.45f), SocketSize.Heavy, SocketMountType.FixedForward, "missile"),
                new ShipEquipmentSlotDefinition("def_0", "Reactive Armor Plating", EquipmentSlotCategory.Defense, new Vector2(0f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_nanite_armor"),
                new ShipEquipmentSlotDefinition("rea_0", "Tachyon Reactor Core", EquipmentSlotCategory.Reactor, new Vector2(-0.80f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "rea_tachyon_core"),
                new ShipEquipmentSlotDefinition("aux_0", "Fire Control Computer", EquipmentSlotCategory.Auxiliary, new Vector2(0.80f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "aux_targeting_comp")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-2.0f, 0.60f),
                new Vector2(-2.0f, -0.60f)
            };
        }

        private static void ConfigureCruiser(ShipDefinition def)
        {
            def.id = "cruiser";
            def.shipName = "Hyperion Cruiser";
            def.tier = 3;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 1.05f;
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
                pulseSpeed = 600f,
                speed = 190f,
                agility = 3.2f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Cruiser_SpineHeavy", new Vector2(2.20f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "beam"),
                new ShipSocketDefinition("Cruiser_TurretL", new Vector2(1.10f, 1.35f), SocketSize.Medium, SocketMountType.Turret, 0f, "gatling"),
                new ShipSocketDefinition("Cruiser_TurretR", new Vector2(1.10f, -1.35f), SocketSize.Medium, SocketMountType.Turret, 0f, "gatling"),
                new ShipSocketDefinition("Cruiser_BroadsideL", new Vector2(-0.30f, 1.65f), SocketSize.Light, SocketMountType.BroadsideLeft, -90f, "pulse"),
                new ShipSocketDefinition("Cruiser_BroadsideR", new Vector2(-0.30f, -1.65f), SocketSize.Light, SocketMountType.BroadsideRight, 90f, "pulse")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Spinal Heavy Beam", EquipmentSlotCategory.Weapon, new Vector2(2.20f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "beam"),
                new ShipEquipmentSlotDefinition("wpn_1", "Port Rotary Turret", EquipmentSlotCategory.Weapon, new Vector2(1.10f, 1.35f), SocketSize.Medium, SocketMountType.Turret, "gatling"),
                new ShipEquipmentSlotDefinition("wpn_2", "Starboard Rotary Turret", EquipmentSlotCategory.Weapon, new Vector2(1.10f, -1.35f), SocketSize.Medium, SocketMountType.Turret, "gatling"),
                new ShipEquipmentSlotDefinition("wpn_3", "Port Broadside Battery", EquipmentSlotCategory.Weapon, new Vector2(-0.30f, 1.65f), SocketSize.Light, SocketMountType.BroadsideLeft, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_4", "Starboard Broadside Battery", EquipmentSlotCategory.Weapon, new Vector2(-0.30f, -1.65f), SocketSize.Light, SocketMountType.BroadsideRight, "pulse"),
                new ShipEquipmentSlotDefinition("def_0", "Primary Aegis Field", EquipmentSlotCategory.Defense, new Vector2(0.30f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_shield_booster"),
                new ShipEquipmentSlotDefinition("def_1", "Reinforced Belt Armor", EquipmentSlotCategory.Defense, new Vector2(-0.30f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_nanite_armor"),
                new ShipEquipmentSlotDefinition("eng_0", "Heavy Ion Drive", EquipmentSlotCategory.Engine, new Vector2(-2.40f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "eng_afterburner"),
                new ShipEquipmentSlotDefinition("rea_0", "Hyperion Power Plant", EquipmentSlotCategory.Reactor, new Vector2(-1.20f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "rea_flux_capacitor")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-2.40f, 0.80f),
                new Vector2(-2.40f, -0.80f),
                new Vector2(-2.60f, 0f)
            };
        }

        private static void ConfigureCarrier(ShipDefinition def)
        {
            def.id = "carrier";
            def.shipName = "Vanguard Carrier";
            def.tier = 3;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 1.25f;
            def.shieldHitboxOffset = 0.35f;
            def.turretTraverseSpeed = 130f;
            def.droneHangarCapacity = 4;
            def.defaultDroneType = "interceptor_drone";
            def.baseStats = new StatValues
            {
                shieldCapacity = 280f,
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
                new ShipSocketDefinition("Carrier_PDC_L", new Vector2(1.80f, 1.20f), SocketSize.Light, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Carrier_PDC_R", new Vector2(1.80f, -1.20f), SocketSize.Light, SocketMountType.Turret, 0f, "pulse"),
                new ShipSocketDefinition("Carrier_HangarBayL", new Vector2(-0.40f, 1.50f), SocketSize.Hangar, SocketMountType.HangarBay, -90f, "hangar"),
                new ShipSocketDefinition("Carrier_HangarBayR", new Vector2(-0.40f, -1.50f), SocketSize.Hangar, SocketMountType.HangarBay, 90f, "hangar")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Port PDC Turret", EquipmentSlotCategory.Weapon, new Vector2(1.80f, 1.20f), SocketSize.Light, SocketMountType.Turret, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_1", "Starboard PDC Turret", EquipmentSlotCategory.Weapon, new Vector2(1.80f, -1.20f), SocketSize.Light, SocketMountType.Turret, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_2", "Port Flight Deck", EquipmentSlotCategory.Weapon, new Vector2(-0.40f, 1.50f), SocketSize.Hangar, SocketMountType.HangarBay, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_3", "Starboard Flight Deck", EquipmentSlotCategory.Weapon, new Vector2(-0.40f, -1.50f), SocketSize.Hangar, SocketMountType.HangarBay, "pulse"),
                new ShipEquipmentSlotDefinition("def_0", "Carrier Shield Grid", EquipmentSlotCategory.Defense, new Vector2(0.50f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_shield_booster"),
                new ShipEquipmentSlotDefinition("rea_0", "Fleet Capacitor Bank", EquipmentSlotCategory.Reactor, new Vector2(-1.20f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "rea_flux_capacitor"),
                new ShipEquipmentSlotDefinition("aux_0", "Escort Drone Foundry", EquipmentSlotCategory.Auxiliary, new Vector2(0f, 0.70f), SocketSize.Heavy, SocketMountType.FixedForward, "aux_escort_dock"),
                new ShipEquipmentSlotDefinition("aux_1", "Fleet Command Telemetry", EquipmentSlotCategory.Auxiliary, new Vector2(0f, -0.70f), SocketSize.Heavy, SocketMountType.FixedForward, "aux_targeting_comp")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-2.80f, 1.0f),
                new Vector2(-2.80f, -1.0f),
                new Vector2(-2.90f, 0f)
            };
        }

        private static void ConfigureBattleship(ShipDefinition def)
        {
            def.id = "battleship";
            def.shipName = "Titan Dreadnought";
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
