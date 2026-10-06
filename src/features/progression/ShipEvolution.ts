import { Entity } from "@/core/ecs/Entity";
import {
  PlayerComponent,
  SpriteComponent,
  HealthComponent,
  EnergyComponent,
  WeaponComponent,
  PhysicsComponent,
  TransformComponent,
} from "@/components";
import { SHIPS_REGISTRY } from "@/config/ships";
import { eventBus } from "@/core/EventBus";

export class ShipEvolution {
  public static canEvolve(playerEntity: Entity): boolean {
    const playerComp = playerEntity.getComponent<PlayerComponent>("Player");
    if (!playerComp) return false;

    const shipDef = SHIPS_REGISTRY[playerComp.currentShipId];
    if (!shipDef || shipDef.evolvesTo.length === 0) return false;

    // Must have maxed out all upgrade levels to evolve (just like Starblast.io!)
    const allMaxed = Object.values(playerComp.upgradeLevels).every(
      (lvl) => lvl >= shipDef.maxUpgradeLevel
    );
    return allMaxed;
  }

  public static getEvolutionChoices(playerEntity: Entity): string[] {
    const playerComp = playerEntity.getComponent<PlayerComponent>("Player");
    if (!playerComp) return [];

    const shipDef = SHIPS_REGISTRY[playerComp.currentShipId];
    return shipDef ? shipDef.evolvesTo : [];
  }

  public static evolve(playerEntity: Entity, targetShipId: string): boolean {
    const playerComp = playerEntity.getComponent<PlayerComponent>("Player");
    if (!playerComp) return false;

    const newShipDef = SHIPS_REGISTRY[targetShipId];
    if (!newShipDef) return false;

    // Reset upgrade levels for the new tier
    playerComp.currentShipId = targetShipId;
    Object.keys(playerComp.upgradeLevels).forEach((k) => {
      (playerComp.upgradeLevels as any)[k] = 0;
    });

    // Update Sprite
    const spriteComp = playerEntity.getComponent<SpriteComponent>("Sprite");
    if (spriteComp && spriteComp.sprite) {
      spriteComp.sprite.setTexture(newShipDef.spriteKey);
      spriteComp.sprite.setScale(newShipDef.scale);
    }

    // Apply Base Stats
    const health = playerEntity.getComponent<HealthComponent>("Health");
    if (health) {
      health.max = newShipDef.baseStats.shieldCapacity;
      health.current = health.max;
      health.regenRate = newShipDef.baseStats.shieldRegen;
    }

    const energy = playerEntity.getComponent<EnergyComponent>("Energy");
    if (energy) {
      energy.max = newShipDef.baseStats.energyCapacity;
      energy.current = energy.max;
      energy.regenRate = newShipDef.baseStats.energyRegen;
    }

    const weapon = playerEntity.getComponent<WeaponComponent>("Weapon");
    if (weapon) {
      weapon.damage = newShipDef.baseStats.pulseDamage;
      weapon.pulseSpeed = newShipDef.baseStats.pulseSpeed;
    }

    const physics = playerEntity.getComponent<PhysicsComponent>("Physics");
    if (physics) {
      physics.maxSpeed = newShipDef.baseStats.speed;
      physics.acceleration = newShipDef.baseStats.speed * 1.5;
      physics.angularVelocity = newShipDef.baseStats.agility;
    }

    eventBus.emit("player:evolved", { newShipId: targetShipId });
    return true;
  }
}
