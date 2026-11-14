using System;
using UnityEngine;

namespace StarStrike.Core
{
    [Serializable]
    public class ShipStats
    {
        public string shipId = "scout";
        public string shipName = "Delta Scout";
        public int tier = 1;
        public int weaponSlots = 1;

        public float shieldCapacity = 100f;
        public float shieldRegen = 8f;
        public float energyCapacity = 80f;
        public float energyRegen = 18f;
        public float pulseDamage = 18f;
        public float pulseSpeed = 22f; // Unity units / sec
        public float speed = 10f;       // Max speed in units/sec
        public float acceleration = 18f;
        public float agility = 6f;      // Turn rate rad/sec

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
                energyCapacity = 150f,
                energyRegen = 20f,
                pulseDamage = 40f,
                pulseSpeed = 19f,
                speed = 8.5f,
                acceleration = 15f,
                agility = 4.5f
            };
        }
    }
}
