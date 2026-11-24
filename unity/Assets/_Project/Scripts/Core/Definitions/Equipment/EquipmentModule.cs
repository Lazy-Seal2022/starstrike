using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Functional categories for modular ship equipment.
    /// </summary>
    public enum EquipmentSlotCategory
    {
        Weapon = 0,
        Defense = 1,
        Engine = 2,
        Reactor = 3,
        Auxiliary = 4
    }

    /// <summary>
    /// Metadata defining a modular equipment slot on a vessel chassis.
    /// </summary>
    [Serializable]
    public struct ShipEquipmentSlotDefinition
    {
        public string slotId;
        public string slotName;
        public EquipmentSlotCategory category;
        public SocketSize size;
        public SocketMountType mountType;
        [Tooltip("Local normalized coordinate offset on the hull preview (-1 to +1)")]
        public Vector2 localPosition;
        public string defaultItemId;

        public ShipEquipmentSlotDefinition(string slotId, string slotName, EquipmentSlotCategory category, Vector2 localPosition, SocketSize size = SocketSize.Medium, SocketMountType mountType = SocketMountType.FixedForward, string defaultItemId = "")
        {
            this.slotId = slotId;
            this.slotName = slotName;
            this.category = category;
            this.localPosition = localPosition;
            this.size = size;
            this.mountType = mountType;
            this.defaultItemId = defaultItemId;
        }
    }

    /// <summary>
    /// Specification for an installable ship module (defense, engine, reactor, auxiliary).
    /// </summary>
    [Serializable]
    public class EquipmentModuleInfo
    {
        public string id;
        public string name;
        public EquipmentSlotCategory category;
        public string role;
        public string description;
        public int gemCost;

        // Defensive Modifiers
        public float shieldCapacityBonus;
        public float shieldRegenBonus;
        public float hullCapacityBonus;
        public float collisionArmorBonus; // e.g. 0.15 = +15% damage reduction

        // Reactor / Energy Modifiers
        public float energyCapacityBonus;
        public float energyRegenBonus;
        public float overdrawResistanceBonus; // reduces lockout penalty

        // Engine / Flight Modifiers
        public float speedBonus;
        public float agilityBonus;

        // Offensive & Utility Modifiers
        public float damageMultiplier = 1.0f; // 1.0 = base
        public float pickupRadiusBonus; // in world units
    }

    /// <summary>
    /// Static catalog and lookup registry for all available modular equipment items.
    /// </summary>
    public static class EquipmentModuleCatalog
    {
        private static readonly Dictionary<string, EquipmentModuleInfo> ModuleMap = new Dictionary<string, EquipmentModuleInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<EquipmentModuleInfo> AllModuleList = new List<EquipmentModuleInfo>();

        static EquipmentModuleCatalog()
        {
            RegisterDefaults();
        }

        private static void Register(EquipmentModuleInfo module)
        {
            if (module == null || string.IsNullOrEmpty(module.id)) return;
            ModuleMap[module.id] = module;
            AllModuleList.Add(module);
        }

        private static void RegisterDefaults()
        {
            // --- DEFENSE MODULES ---
            Register(new EquipmentModuleInfo
            {
                id = "def_shield_booster",
                name = "Shield Overcharger",
                category = EquipmentSlotCategory.Defense,
                role = "DEFENSE // SHIELD AMPLIFICATION",
                description = "Secondary plasma emitters boosting total shield threshold and cycle regeneration rate.",
                gemCost = 400,
                shieldCapacityBonus = 50f,
                shieldRegenBonus = 6f
            });

            Register(new EquipmentModuleInfo
            {
                id = "def_nanite_armor",
                name = "Nanite Reactive Armor",
                category = EquipmentSlotCategory.Defense,
                role = "DEFENSE // REINFORCED HULL",
                description = "Self-repairing composite titanium plates delivering massive structural integrity and kinetic absorption.",
                gemCost = 500,
                hullCapacityBonus = 100f,
                collisionArmorBonus = 0.20f
            });

            Register(new EquipmentModuleInfo
            {
                id = "def_phase_deflector",
                name = "Phase Deflector Grid",
                category = EquipmentSlotCategory.Defense,
                role = "DEFENSE // HYBRID SHIELD & BULKHEAD",
                description = "Harmonic phase barriers that deflect high-impact collisions and disperse energy waves.",
                gemCost = 650,
                shieldCapacityBonus = 70f,
                hullCapacityBonus = 40f,
                collisionArmorBonus = 0.12f
            });

            // --- ENGINE MODULES ---
            Register(new EquipmentModuleInfo
            {
                id = "eng_afterburner",
                name = "High-Flow Afterburner",
                category = EquipmentSlotCategory.Engine,
                role = "PROPULSION // CRUISE VELOCITY",
                description = "Superheated fuel injection nozzles granting superior sublight combat cruising speed.",
                gemCost = 350,
                speedBonus = 45f,
                agilityBonus = 0.8f
            });

            Register(new EquipmentModuleInfo
            {
                id = "eng_vector_thrusters",
                name = "Gyroscopic Vectoring",
                category = EquipmentSlotCategory.Engine,
                role = "PROPULSION // ANGULAR AGILITY",
                description = "Precision RCS vernier thrusters providing instantaneous rotational authority in dogfights.",
                gemCost = 450,
                speedBonus = 20f,
                agilityBonus = 2.4f
            });

            Register(new EquipmentModuleInfo
            {
                id = "eng_micro_warp",
                name = "Micro-Warp Drives",
                category = EquipmentSlotCategory.Engine,
                role = "PROPULSION // WARP MOMENTUM",
                description = "Localized spacetime warping manifold yielding extreme forward combat velocity.",
                gemCost = 700,
                speedBonus = 65f,
                agilityBonus = 0.4f,
                energyRegenBonus = 4f
            });

            // --- REACTOR MODULES ---
            Register(new EquipmentModuleInfo
            {
                id = "rea_flux_capacitor",
                name = "Supercharged Flux Core",
                category = EquipmentSlotCategory.Reactor,
                role = "POWER // CAPACITOR POOL",
                description = "High-density plasma cell expanding maximum ship capacitor reservoir for protracted fire.",
                gemCost = 400,
                energyCapacityBonus = 60f,
                energyRegenBonus = 4f
            });

            Register(new EquipmentModuleInfo
            {
                id = "rea_tachyon_core",
                name = "Tachyon Fast-Recharger",
                category = EquipmentSlotCategory.Reactor,
                role = "POWER // FAST CYCLING",
                description = "Tachyon circulation loop multiplying energy replenishment speed for rapid burst recovery.",
                gemCost = 550,
                energyCapacityBonus = 20f,
                energyRegenBonus = 14f
            });

            Register(new EquipmentModuleInfo
            {
                id = "rea_overdraw_surge",
                name = "Overdraw Surge Matrix",
                category = EquipmentSlotCategory.Reactor,
                role = "POWER // LOCKOUT MITIGATION",
                description = "Reinforced shunt conduits that prevent severe capacitor burnout when performing emergency overdraws.",
                gemCost = 600,
                energyCapacityBonus = 35f,
                energyRegenBonus = 8f,
                overdrawResistanceBonus = 0.35f
            });

            // --- AUXILIARY MODULES ---
            Register(new EquipmentModuleInfo
            {
                id = "aux_graviton_siphon",
                name = "Graviton Siphon",
                category = EquipmentSlotCategory.Auxiliary,
                role = "AUXILIARY // RESOURCE COLLECTION",
                description = "Electromagnetic attraction emitter pulling loose meta-gems and ordnance crystals from afar.",
                gemCost = 350,
                pickupRadiusBonus = 5.0f,
                energyCapacityBonus = 15f
            });

            Register(new EquipmentModuleInfo
            {
                id = "aux_targeting_comp",
                name = "Fire Control Array",
                category = EquipmentSlotCategory.Auxiliary,
                role = "AUXILIARY // FIREPOWER AMPLIFICATION",
                description = "Predictive telemetry mainframe optimizing fire solution timing for all mounted weapon hardpoints.",
                gemCost = 650,
                damageMultiplier = 1.18f,
                speedBonus = 10f
            });

            Register(new EquipmentModuleInfo
            {
                id = "aux_escort_dock",
                name = "Escort Drone Foundry",
                category = EquipmentSlotCategory.Auxiliary,
                role = "AUXILIARY // SQUADRON SUPPORT",
                description = "Automated fabrication dock constructing autonomous combat drones to flank hostiles.",
                gemCost = 800,
                shieldCapacityBonus = 25f,
                damageMultiplier = 1.10f
            });
        }

        public static EquipmentModuleInfo GetModule(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (ModuleMap.TryGetValue(id, out var info)) return info;
            return null;
        }

        public static List<EquipmentModuleInfo> GetModulesByCategory(EquipmentSlotCategory category)
        {
            var list = new List<EquipmentModuleInfo>();
            for (int i = 0; i < AllModuleList.Count; i++)
            {
                if (AllModuleList[i].category == category)
                {
                    list.Add(AllModuleList[i]);
                }
            }
            return list;
        }

        public static IReadOnlyList<EquipmentModuleInfo> AllModules => AllModuleList;
    }
}
