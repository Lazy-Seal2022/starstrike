import { ShipDefinition } from "../types";

export const GAME_CONFIG = {
  // World bounds
  world: {
    width: 4000,
    height: 4000,
  },
  // Physics parameters (Newtonian flight feel like Starblast.io)
  physics: {
    baseFriction: 0.985, // Space drag
    maxVelocity: 500,
    rotationSpeed: 0.08, // radians per frame
  },
  // Mining & Asteroid field parameters
  mining: {
    asteroidCount: 80,
    gemMagnetRadius: 180,
    gemMagnetSpeed: 380,
    asteroidHealth: {
      large: 120,
      medium: 50,
      small: 20,
    },
    asteroidGems: {
      large: 8,
      medium: 4,
      small: 1,
    },
  },
  // Upgrade multiplier constants
  upgradeMultipliers: {
    shieldCapacity: 1.25,
    shieldRegen: 1.3,
    energyCapacity: 1.2,
    energyRegen: 1.35,
    pulseDamage: 1.25,
    pulseSpeed: 1.15,
    speed: 1.12,
    agility: 1.15,
  },
  upgradeBaseCost: 10,
  upgradeCostMultiplier: 1.6,
};

export const SHIPS_REGISTRY: Record<string, ShipDefinition> = {
  scout: {
    id: "scout",
    name: "Delta Scout",
    tier: 1,
    baseStats: {
      shieldCapacity: 100,
      shieldRegen: 8,
      energyCapacity: 80,
      energyRegen: 18,
      pulseDamage: 18,
      pulseSpeed: 600,
      speed: 260,
      agility: 4.5,
    },
    maxUpgradeLevel: 4,
    evolvesTo: ["fighter", "miner"],
    weaponSlots: 1,
    spriteKey: "ship_scout",
    color: 0x38bdf8, // Cyan-blue
    scale: 1.0,
  },
  fighter: {
    id: "fighter",
    name: "Ares Fighter",
    tier: 2,
    baseStats: {
      shieldCapacity: 150,
      shieldRegen: 12,
      energyCapacity: 120,
      energyRegen: 24,
      pulseDamage: 28,
      pulseSpeed: 680,
      speed: 310,
      agility: 5.0,
    },
    maxUpgradeLevel: 5,
    evolvesTo: ["interceptor", "destroyer"],
    weaponSlots: 2,
    spriteKey: "ship_fighter",
    color: 0xf43f5e, // Rose-red
    scale: 1.2,
  },
  miner: {
    id: "miner",
    name: "Goliath Miner",
    tier: 2,
    baseStats: {
      shieldCapacity: 220,
      shieldRegen: 16,
      energyCapacity: 150,
      energyRegen: 20,
      pulseDamage: 40,
      pulseSpeed: 520,
      speed: 210,
      agility: 3.5,
    },
    maxUpgradeLevel: 5,
    evolvesTo: ["colossus", "dreadnought"],
    weaponSlots: 2,
    spriteKey: "ship_miner",
    color: 0xf59e0b, // Amber-gold
    scale: 1.35,
  },
};
