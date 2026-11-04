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
