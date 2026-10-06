import { ShipDefinition } from "@/core/types";

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
    evolvesTo: [],
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
    evolvesTo: [],
    weaponSlots: 2,
    spriteKey: "ship_miner",
    color: 0xf59e0b, // Amber-gold
    scale: 1.35,
  },
};
