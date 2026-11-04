export type ShipTier = 1 | 2 | 3 | 4 | 5 | 6 | 7;

export type UpgradeKey =
  | "shieldCapacity"
  | "shieldRegen"
  | "energyCapacity"
  | "energyRegen"
  | "pulseDamage"
  | "pulseSpeed"
  | "speed"
  | "agility";

export interface ShipStats {
  shieldCapacity: number;
  shieldRegen: number;
  energyCapacity: number;
  energyRegen: number;
  pulseDamage: number;
  pulseSpeed: number;
  speed: number;
  agility: number;
}

export interface UpgradeLevels {
  shieldCapacity: number;
  shieldRegen: number;
  energyCapacity: number;
  energyRegen: number;
  pulseDamage: number;
  pulseSpeed: number;
  speed: number;
  agility: number;
}

export interface ShipDefinition {
  id: string;
  name: string;
  tier: ShipTier;
  baseStats: ShipStats;
  maxUpgradeLevel: number;
  evolvesTo: string[];
  weaponSlots: number;
  spriteKey: string;
  color: number;
  scale: number;
}

export interface GameEvents {
  "player:spawned": { x: number; y: number };
  "player:died": { reason: string };
  "player:upgraded": { key: UpgradeKey; level: number; cost: number };
  "player:evolved": { newShipId: string };
  "gem:collected": { amount: number; total: number };
  "asteroid:destroyed": { x: number; y: number; size: "large" | "medium" | "small" };
  "energy:changed": { current: number; max: number };
  "shield:changed": { current: number; max: number };
}
