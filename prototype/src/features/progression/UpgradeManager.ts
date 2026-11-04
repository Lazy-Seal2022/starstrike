import { Entity } from "@/core/ecs/Entity";
import {
  PlayerComponent,
  HealthComponent,
  EnergyComponent,
  WeaponComponent,
  PhysicsComponent,
} from "@/components";
import { UpgradeKey } from "@/core/types";
import { GAME_CONFIG } from "@/config/game";
import { SHIPS_REGISTRY } from "@/config/ships";
import { eventBus } from "@/core/EventBus";

export class UpgradeManager {
  public static getUpgradeCost(level: number): number {
    return Math.floor(
      GAME_CONFIG.upgradeBaseCost * Math.pow(GAME_CONFIG.upgradeCostMultiplier, level)
    );
  }

  public static canUpgrade(playerEntity: Entity, key: UpgradeKey): boolean {
    const playerComp = playerEntity.getComponent<PlayerComponent>("Player");
    if (!playerComp) return false;

    const shipDef = SHIPS_REGISTRY[playerComp.currentShipId];
    if (!shipDef) return false;

    const currentLevel = playerComp.upgradeLevels[key];
    if (currentLevel >= shipDef.maxUpgradeLevel) return false;

    const cost = this.getUpgradeCost(currentLevel);
    return playerComp.gems >= cost;
  }

  public static upgrade(playerEntity: Entity, key: UpgradeKey): boolean {
    if (!this.canUpgrade(playerEntity, key)) return false;

    const playerComp = playerEntity.getComponent<PlayerComponent>("Player")!;
    const currentLevel = playerComp.upgradeLevels[key];
    const cost = this.getUpgradeCost(currentLevel);

    // Deduct gems
    playerComp.gems -= cost;
    playerComp.upgradeLevels[key]++;
    const newLevel = playerComp.upgradeLevels[key];

    // Apply stat increase to components
    const mult = GAME_CONFIG.upgradeMultipliers[key];

    switch (key) {
      case "shieldCapacity": {
        const health = playerEntity.getComponent<HealthComponent>("Health");
        if (health) {
          health.max *= mult;
          health.current = Math.min(health.max, health.current * mult);
        }
        break;
      }
      case "shieldRegen": {
        const health = playerEntity.getComponent<HealthComponent>("Health");
        if (health) health.regenRate *= mult;
        break;
      }
      case "energyCapacity": {
        const energy = playerEntity.getComponent<EnergyComponent>("Energy");
        if (energy) {
          energy.max *= mult;
          energy.current = Math.min(energy.max, energy.current * mult);
        }
        break;
      }
      case "energyRegen": {
        const energy = playerEntity.getComponent<EnergyComponent>("Energy");
        if (energy) energy.regenRate *= mult;
        break;
      }
      case "pulseDamage": {
        const weapon = playerEntity.getComponent<WeaponComponent>("Weapon");
        if (weapon) weapon.damage *= mult;
        break;
      }
      case "pulseSpeed": {
        const weapon = playerEntity.getComponent<WeaponComponent>("Weapon");
        if (weapon) weapon.pulseSpeed *= mult;
        break;
      }
      case "speed": {
        const physics = playerEntity.getComponent<PhysicsComponent>("Physics");
        if (physics) {
          physics.maxSpeed *= mult;
          physics.acceleration *= mult;
        }
        break;
      }
      case "agility": {
        const physics = playerEntity.getComponent<PhysicsComponent>("Physics");
        if (physics) physics.angularVelocity *= mult;
        break;
      }
    }

    eventBus.emit("player:upgraded", { key, level: newLevel, cost });
    eventBus.emit("gem:collected", { amount: 0, total: playerComp.gems });
    return true;
  }
}
