using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval Class Builder: Destroyer (Anti-Capital Heavy Gunship Hull Class).
    /// Emphasizes forward-mounted kinetic alpha strikes, heavy torpedo pods, and aggressive hull durability.
    /// </summary>
    public static class DestroyerClass
    {
        public static void ConfigureDestroyer(ShipDefinition def)
        {
            def.id = "destroyer";
            def.shipName = "Cerberus Destroyer";
            def.shipClass = ShipClass.Destroyer;
            def.tier = 2;
            def.maxUpgradeLevel = 6;
            def.hitboxRadius = 0.85f;
            def.shieldHitboxOffset = 0.25f;
            def.turretTraverseSpeed = 150f;
            def.baseStats = new StatValues
            {
                shieldCapacity = 240f,
                shieldRegen = 18f,
                hullCapacity = 220f,
                energyCapacity = 140f,
                energyRegen = 24f,
                pulseDamage = 45f,
                pulseSpeed = 620f,
                speed = 240f,
                agility = 4.0f
            };
            def.hardpointSockets = new[]
            {
                new ShipSocketDefinition("Destroyer_MainBatteryL", new Vector2(1.85f, 0.65f), SocketSize.Medium, SocketMountType.FixedForward, 0f, "pulse"),
                new ShipSocketDefinition("Destroyer_MainBatteryR", new Vector2(1.85f, -0.65f), SocketSize.Medium, SocketMountType.FixedForward, 0f, "pulse"),
                new ShipSocketDefinition("Destroyer_TorpedoPodL", new Vector2(-0.10f, 1.30f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "missile"),
                new ShipSocketDefinition("Destroyer_TorpedoPodR", new Vector2(-0.10f, -1.30f), SocketSize.Heavy, SocketMountType.FixedForward, 0f, "missile")
            };
            def.equipmentSlots = new[]
            {
                new ShipEquipmentSlotDefinition("wpn_0", "Port Kinetic Battery", EquipmentSlotCategory.Weapon, new Vector2(1.85f, 0.65f), SocketSize.Medium, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_1", "Starboard Kinetic Battery", EquipmentSlotCategory.Weapon, new Vector2(1.85f, -0.65f), SocketSize.Medium, SocketMountType.FixedForward, "pulse"),
                new ShipEquipmentSlotDefinition("wpn_2", "Port Heavy Torpedo Bay", EquipmentSlotCategory.Weapon, new Vector2(-0.10f, 1.30f), SocketSize.Heavy, SocketMountType.FixedForward, "missile"),
                new ShipEquipmentSlotDefinition("wpn_3", "Starboard Heavy Torpedo Bay", EquipmentSlotCategory.Weapon, new Vector2(-0.10f, -1.30f), SocketSize.Heavy, SocketMountType.FixedForward, "missile"),
                new ShipEquipmentSlotDefinition("def_0", "Heavy Blast Plating", EquipmentSlotCategory.Defense, new Vector2(-0.50f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "def_nanite_armor"),
                new ShipEquipmentSlotDefinition("rea_0", "Heavy Overcharged Core", EquipmentSlotCategory.Reactor, new Vector2(-1.10f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "rea_flux_capacitor"),
                new ShipEquipmentSlotDefinition("eng_0", "Impulse Thruster Array", EquipmentSlotCategory.Engine, new Vector2(-1.90f, 0f), SocketSize.Heavy, SocketMountType.FixedForward, "eng_afterburner")
            };
            def.thrusterSockets = new[]
            {
                new Vector2(-1.90f, 0.60f),
                new Vector2(-1.90f, -0.60f),
                new Vector2(-2.00f, 0f)
            };
        }
    }
}
