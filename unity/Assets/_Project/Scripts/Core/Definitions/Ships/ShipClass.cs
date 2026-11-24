using System;
using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Naval/space combat ship classification hierarchy.
    /// </summary>
    public enum ShipClass
    {
        Fighter = 0,
        Frigate = 1,
        Destroyer = 2,
        Cruiser = 3,
        Carrier = 4,
        Battleship = 5
    }

    /// <summary>
    /// Classification of weapon hardpoint capacity and socket mount sizing.
    /// </summary>
    public enum SocketSize
    {
        Light = 0,
        Medium = 1,
        Heavy = 2,
        Spinal = 3,
        Hangar = 4
    }

    /// <summary>
    /// Firing arc and physical mounting geometry on the vessel hull.
    /// </summary>
    public enum SocketMountType
    {
        FixedForward = 0,
        Turret = 1,
        BroadsideLeft = 2,
        BroadsideRight = 3,
        Spinal = 4,
        HangarBay = 5
    }

    /// <summary>
    /// Rich hardpoint socket metadata defining physical offset, mount angle, size, and archetype.
    /// </summary>
    [Serializable]
    public struct ShipSocketDefinition
    {
        public string socketId;
        public Vector2 localPosition;
        [Tooltip("Mount heading offset in degrees (0 = forward, 90 = right broadside, -90 = left broadside, 180 = rear)")]
        public float mountAngle;
        public SocketSize size;
        public SocketMountType mountType;
        public string defaultWeaponId;

        public ShipSocketDefinition(string socketId, Vector2 localPosition, SocketSize size = SocketSize.Medium, SocketMountType mountType = SocketMountType.FixedForward, float mountAngle = 0f, string defaultWeaponId = "pulse")
        {
            this.socketId = socketId;
            this.localPosition = localPosition;
            this.size = size;
            this.mountType = mountType;
            this.mountAngle = mountAngle;
            this.defaultWeaponId = defaultWeaponId;
        }
    }

    /// <summary>
    /// Helper extension methods providing archetype defaults and design parameters.
    /// </summary>
    public static class ShipClassExtensions
    {
        public static string GetDisplayName(this ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => "Fighter",
                ShipClass.Frigate => "Frigate",
                ShipClass.Destroyer => "Destroyer",
                ShipClass.Cruiser => "Cruiser",
                ShipClass.Carrier => "Carrier",
                ShipClass.Battleship => "Battleship",
                _ => sc.ToString()
            };
        }

        public static string GetClassRoleDescription(this ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => "High-agility strike interceptor, rapid capacitor reboot, glass cannon dogfighter.",
                ShipClass.Frigate => "Fast wolfpack raider and escort, tactical EMP/cloak flanker.",
                ShipClass.Destroyer => "Anti-capital heavy gunship, massive forward torpedoes, high kinetic alpha strike.",
                ShipClass.Cruiser => "Fleet backbone battleline warship, 360-degree turret coverage, heavy energy aegis.",
                ShipClass.Carrier => "Fleet hive commander, automated drone foundry, remote strike squadrons.",
                ShipClass.Battleship => "Superheavy dreadnought fortress, colossal spinal superweapon, extreme armor mass.",
                _ => "Combat starship"
            };
        }

        public static float GetDefaultCameraSize(this ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => 7.5f,
                ShipClass.Frigate => 9.0f,
                ShipClass.Destroyer => 10.5f,
                ShipClass.Cruiser => 12.0f,
                ShipClass.Carrier => 13.5f,
                ShipClass.Battleship => 15.0f,
                _ => 8.5f
            };
        }

        public static float GetDefaultHullMass(this ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => 1.0f,
                ShipClass.Frigate => 1.6f,
                ShipClass.Destroyer => 2.5f,
                ShipClass.Cruiser => 4.0f,
                ShipClass.Carrier => 5.5f,
                ShipClass.Battleship => 9.0f,
                _ => 1.0f
            };
        }

        public static float GetCollisionArmorReduction(this ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => 0.0f,
                ShipClass.Frigate => 0.05f,
                ShipClass.Destroyer => 0.10f,
                ShipClass.Cruiser => 0.20f,
                ShipClass.Carrier => 0.15f,
                ShipClass.Battleship => 0.40f,
                _ => 0.0f
            };
        }

        public static int GetBaseMaxSockets(this ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => 2,
                ShipClass.Frigate => 3,
                ShipClass.Destroyer => 4,
                ShipClass.Cruiser => 5,
                ShipClass.Carrier => 4,
                ShipClass.Battleship => 6,
                _ => 2
            };
        }
    }
}
