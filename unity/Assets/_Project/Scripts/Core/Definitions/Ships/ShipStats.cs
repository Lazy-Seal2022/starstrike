using System;
using UnityEngine;

namespace StarStrike.Core
{
    [Serializable]
    public class ShipStats
    {
        public string shipId = "scout";
        public string shipName = "Delta Scout";
        public ShipClass shipClass = ShipClass.Fighter;
        public int tier = 1;
        public int weaponSlots = 1;

        public float shieldCapacity = 100f;
        public float shieldRegen = 8f;
        public float hullCapacity = 80f;
        public float energyCapacity = 80f;
        public float energyRegen = 18f;
        public float pulseDamage = 18f;
        public float pulseSpeed = 22f; // Unity units / sec
        public float speed = 10f;       // Max speed in units/sec
        public float acceleration = 18f;
        public float agility = 6f;      // Turn rate rad/sec
        public float cameraOrthoSize = 8.5f;
        public float hullMass = 1.0f;
        public float collisionArmorReduction = 0f;
        public int droneHangarCapacity = 0;

        public static ShipStats CreateScout()
        {
            return new ShipStats
            {
                shipId = "scout",
                shipName = "Delta Scout",
                tier = 1,
                weaponSlots = 1,
                shieldCapacity = 100f,
                shieldRegen = 8f,
                hullCapacity = 80f,
                energyCapacity = 80f,
                energyRegen = 18f,
                pulseDamage = 18f,
                pulseSpeed = 22f,
                speed = 10f,
                acceleration = 18f,
                agility = 6f
            };
        }

        public static ShipStats CreateFighter()
        {
            return new ShipStats
            {
                shipId = "fighter",
                shipName = "Ares Fighter",
                tier = 2,
                weaponSlots = 2,
                shieldCapacity = 150f,
                shieldRegen = 12f,
                hullCapacity = 120f,
                energyCapacity = 120f,
                energyRegen = 24f,
                pulseDamage = 28f,
                pulseSpeed = 26f,
                speed = 13f,
                acceleration = 24f,
                agility = 7f
            };
        }

        public static ShipStats CreateMiner()
        {
            return new ShipStats
            {
                shipId = "miner",
                shipName = "Goliath Miner",
                tier = 2,
                weaponSlots = 2,
                shieldCapacity = 220f,
                shieldRegen = 16f,
                hullCapacity = 180f,
                energyCapacity = 150f,
                energyRegen = 20f,
                pulseDamage = 40f,
                pulseSpeed = 19f,
                speed = 8.5f,
                acceleration = 15f,
                agility = 4.5f
            };
        }
        public static ShipStats CreateFrigate()
        {
            return new ShipStats
            {
                shipId = "frigate",
                shipName = "Viper Frigate",
                shipClass = ShipClass.Frigate,
                tier = 2,
                weaponSlots = 3,
                shieldCapacity = 180f,
                shieldRegen = 16f,
                hullCapacity = 140f,
                energyCapacity = 110f,
                energyRegen = 22f,
                pulseDamage = 32f,
                pulseSpeed = 25f,
                speed = 11.5f,
                acceleration = 20f,
                agility = 5.8f,
                cameraOrthoSize = 9.0f,
                hullMass = 1.6f,
                collisionArmorReduction = 0.05f
            };
        }

        public static ShipStats CreateDestroyer()
        {
            return new ShipStats
            {
                shipId = "destroyer",
                shipName = "Goliath Destroyer",
                shipClass = ShipClass.Destroyer,
                tier = 2,
                weaponSlots = 4,
                shieldCapacity = 240f,
                shieldRegen = 18f,
                hullCapacity = 220f,
                energyCapacity = 140f,
                energyRegen = 20f,
                pulseDamage = 45f,
                pulseSpeed = 20f,
                speed = 9.5f,
                acceleration = 16f,
                agility = 4.2f,
                cameraOrthoSize = 10.5f,
                hullMass = 2.5f,
                collisionArmorReduction = 0.10f
            };
        }

        public static ShipStats CreateCruiser()
        {
            return new ShipStats
            {
                shipId = "cruiser",
                shipName = "Hyperion Cruiser",
                shipClass = ShipClass.Cruiser,
                tier = 3,
                weaponSlots = 5,
                shieldCapacity = 350f,
                shieldRegen = 22f,
                hullCapacity = 320f,
                energyCapacity = 180f,
                energyRegen = 24f,
                pulseDamage = 55f,
                pulseSpeed = 22f,
                speed = 7.5f,
                acceleration = 12f,
                agility = 3.2f,
                cameraOrthoSize = 12.0f,
                hullMass = 4.0f,
                collisionArmorReduction = 0.20f
            };
        }

        public static ShipStats CreateCarrier()
        {
            return new ShipStats
            {
                shipId = "carrier",
                shipName = "Vanguard Carrier",
                shipClass = ShipClass.Carrier,
                tier = 3,
                weaponSlots = 4,
                shieldCapacity = 280f,
                shieldRegen = 18f,
                hullCapacity = 400f,
                energyCapacity = 220f,
                energyRegen = 25f,
                pulseDamage = 30f,
                pulseSpeed = 22f,
                speed = 6.5f,
                acceleration = 10f,
                agility = 2.6f,
                cameraOrthoSize = 13.5f,
                hullMass = 5.5f,
                collisionArmorReduction = 0.15f,
                droneHangarCapacity = 4
            };
        }

        public static ShipStats CreateBattleship()
        {
            return new ShipStats
            {
                shipId = "battleship",
                shipName = "Titan Dreadnought",
                shipClass = ShipClass.Battleship,
                tier = 3,
                weaponSlots = 6,
                shieldCapacity = 500f,
                shieldRegen = 28f,
                hullCapacity = 600f,
                energyCapacity = 300f,
                energyRegen = 20f,
                pulseDamage = 75f,
                pulseSpeed = 20f,
                speed = 5.0f,
                acceleration = 7f,
                agility = 1.8f,
                cameraOrthoSize = 15.0f,
                hullMass = 9.0f,
                collisionArmorReduction = 0.40f
            };
        }

        public static ShipStats CreateByClass(ShipClass sc)
        {
            return sc switch
            {
                ShipClass.Fighter => CreateFighter(),
                ShipClass.Frigate => CreateFrigate(),
                ShipClass.Destroyer => CreateDestroyer(),
                ShipClass.Cruiser => CreateCruiser(),
                ShipClass.Carrier => CreateCarrier(),
                ShipClass.Battleship => CreateBattleship(),
                _ => CreateFighter()
            };
        }
    }
}
