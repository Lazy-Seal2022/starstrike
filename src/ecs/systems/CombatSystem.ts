import Phaser from "phaser";
import { System } from "../System";
import { World } from "../World";
import {
  TransformComponent,
  PhysicsComponent,
  LaserComponent,
  AsteroidComponent,
  SpriteComponent,
  HealthComponent,
  PlayerComponent,
  GemComponent,
} from "../components";
import { eventBus } from "../../core/EventBus";

export class CombatSystem implements System {
  constructor(private scene: Phaser.Scene) {}

  public update(world: World, delta: number): void {
    const dt = delta / 1000;
    const lasers = world.query("Laser", "Transform", "Physics");
    const asteroids = world.query("Asteroid", "Transform");
    const drones = world.query("AI", "Transform", "Health");
    const players = world.query("Player", "Transform", "Health");

    for (const laserEntity of lasers) {
      if (!laserEntity.isAlive) continue;

      const laser = laserEntity.getComponent<LaserComponent>("Laser")!;
      const laserTransform = laserEntity.getComponent<TransformComponent>("Transform")!;
      const laserPhysics = laserEntity.getComponent<PhysicsComponent>("Physics")!;

      // Track distance traveled
      const dist = Math.sqrt(laserPhysics.vx * laserPhysics.vx + laserPhysics.vy * laserPhysics.vy) * dt;
      laser.distanceTraveled += dist;

      if (laser.distanceTraveled >= laser.maxRange) {
        this.destroyEntitySprite(laserEntity);
        world.removeEntity(laserEntity.id);
        continue;
      }

      // Case A: Enemy Laser targeting Player
      if (laser.isEnemy) {
        if (players.length > 0 && players[0].isAlive) {
          const player = players[0];
          const pTransform = player.getComponent<TransformComponent>("Transform")!;
          const pHealth = player.getComponent<HealthComponent>("Health")!;

          const dx = laserTransform.x - pTransform.x;
          const dy = laserTransform.y - pTransform.y;
          const distance = Math.sqrt(dx * dx + dy * dy);

          if (distance < 24) {
            // Player hit by enemy laser!
            this.createImpactSparks(laserTransform.x, laserTransform.y, 0xef4444);

            pHealth.current = Math.max(0, pHealth.current - laser.damage);
            eventBus.emit("shield:changed", { current: pHealth.current, max: pHealth.max });

            // Camera shake
            this.scene.cameras.main.shake(120, 0.007);

            // Flash player red
            const pSpriteComp = player.getComponent<SpriteComponent>("Sprite");
            if (pSpriteComp && pSpriteComp.sprite) {
              pSpriteComp.sprite.setTint(0xef4444);
              this.scene.time.delayedCall(80, () => {
                if (pSpriteComp.sprite && pSpriteComp.sprite.active) {
                  pSpriteComp.sprite.clearTint();
                }
              });
            }

            // Destroy laser
            this.destroyEntitySprite(laserEntity);
            world.removeEntity(laserEntity.id);

            // Player Shield Depletion / Respawn check
            if (pHealth.current <= 0) {
              this.handlePlayerDeath(player, pTransform);
            }
            continue;
          }
        }
      }

      // Case B: Player Laser targeting Asteroids & Enemy Drones
      if (!laser.isEnemy) {
        let laserConsumed = false;

        // 1. Check Drones
        for (const drone of drones) {
          if (!drone.isAlive) continue;

          const dTransform = drone.getComponent<TransformComponent>("Transform")!;
          const dHealth = drone.getComponent<HealthComponent>("Health")!;

          const dx = laserTransform.x - dTransform.x;
          const dy = laserTransform.y - dTransform.y;
          const distance = Math.sqrt(dx * dx + dy * dy);

          if (distance < 26) {
            // Drone hit!
            this.createImpactSparks(laserTransform.x, laserTransform.y, 0x38bdf8);
            dHealth.current -= laser.damage;

            // Flash drone white
            const dSpriteComp = drone.getComponent<SpriteComponent>("Sprite");
            if (dSpriteComp && dSpriteComp.sprite) {
              dSpriteComp.sprite.setTint(0xffffff);
              this.scene.time.delayedCall(60, () => {
                if (dSpriteComp.sprite && dSpriteComp.sprite.active) {
                  dSpriteComp.sprite.clearTint();
                }
              });
            }

            // Destroy laser
            this.destroyEntitySprite(laserEntity);
            world.removeEntity(laserEntity.id);
            laserConsumed = true;

            // Check drone destroyed
            if (dHealth.current <= 0) {
              this.handleDroneDestroyed(world, dTransform.x, dTransform.y);
              this.destroyEntitySprite(drone);
              world.removeEntity(drone.id);
            }
            break;
          }
        }

        if (laserConsumed) continue;

        // 2. Check Asteroids
        for (const asteroidEntity of asteroids) {
          if (!asteroidEntity.isAlive) continue;

          const astTransform = asteroidEntity.getComponent<TransformComponent>("Transform")!;
          const asteroid = asteroidEntity.getComponent<AsteroidComponent>("Asteroid")!;

          const radiusMap = { large: 28, medium: 17, small: 10 };
          const radius = radiusMap[asteroid.size] || 15;

          const dx = laserTransform.x - astTransform.x;
          const dy = laserTransform.y - astTransform.y;
          const distance = Math.sqrt(dx * dx + dy * dy);

          if (distance < radius + 6) {
            this.createImpactSparks(laserTransform.x, laserTransform.y, 0xf59e0b);
            asteroid.health -= laser.damage;

            // Flash asteroid white briefly
            const astSpriteComp = asteroidEntity.getComponent<SpriteComponent>("Sprite");
            if (astSpriteComp && astSpriteComp.sprite) {
              astSpriteComp.sprite.setTint(0xffffff);
              this.scene.time.delayedCall(60, () => {
                if (astSpriteComp.sprite && astSpriteComp.sprite.active) {
                  astSpriteComp.sprite.clearTint();
                }
              });
            }

            // Destroy laser
            this.destroyEntitySprite(laserEntity);
            world.removeEntity(laserEntity.id);

            // Check if asteroid destroyed
            if (asteroid.health <= 0) {
              eventBus.emit("asteroid:destroyed", {
                x: astTransform.x,
                y: astTransform.y,
                size: asteroid.size,
              });
              this.destroyEntitySprite(asteroidEntity);
              world.removeEntity(asteroidEntity.id);
            }
            break;
          }
        }
      }
    }
  }

  private handleDroneDestroyed(world: World, x: number, y: number): void {
    // Large fiery explosion
    const emitter = this.scene.add.particles(x, y, "spark_particle", {
      speed: { min: 80, max: 260 },
      angle: { min: 0, max: 360 },
      scale: { start: 1.5, end: 0 },
      tint: 0xef4444,
      lifespan: 500,
      quantity: 18,
      emitting: false,
    });
    emitter.explode(18);
    this.scene.time.delayedCall(600, () => emitter.destroy());

    // Drop 6 high-value crystals
    for (let i = 0; i < 6; i++) {
      const gemEntity = world.createEntity();
      const sprite = this.scene.add.sprite(x, y, "gem_crystal");
      const angle = Math.random() * Math.PI * 2;
      const speed = Phaser.Math.Between(60, 160);

      gemEntity.addComponent(new TransformComponent(x, y, 0));
      gemEntity.addComponent(new PhysicsComponent(0, 400, 0.95, 0));
      gemEntity.addComponent(new SpriteComponent(sprite));
      gemEntity.addComponent(new GemComponent(2)); // High-tier gems!

      const physics = gemEntity.getComponent<PhysicsComponent>("Physics")!;
      physics.vx = Math.cos(angle) * speed;
      physics.vy = Math.sin(angle) * speed;
    }
  }

  private handlePlayerDeath(player: any, pTransform: TransformComponent): void {
    // Explosion on player
    const emitter = this.scene.add.particles(pTransform.x, pTransform.y, "spark_particle", {
      speed: { min: 90, max: 280 },
      scale: { start: 2, end: 0 },
      tint: 0x38bdf8,
      lifespan: 600,
      quantity: 24,
      emitting: false,
    });
    emitter.explode(24);
    this.scene.time.delayedCall(700, () => emitter.destroy());

    // Respawn after 1.5 seconds at center with full shield
    this.scene.time.delayedCall(1500, () => {
      const health = player.getComponent("Health");
      const physics = player.getComponent("Physics");
      if (health) {
        health.current = health.max;
        eventBus.emit("shield:changed", { current: health.current, max: health.max });
      }
      if (physics) {
        physics.vx = 0;
        physics.vy = 0;
      }
      pTransform.x = 0;
      pTransform.y = 0;
    });
  }

  private createImpactSparks(x: number, y: number, tint: number = 0xf59e0b): void {
    const emitter = this.scene.add.particles(x, y, "spark_particle", {
      speed: { min: 60, max: 180 },
      angle: { min: 0, max: 360 },
      scale: { start: 1, end: 0 },
      tint: tint,
      lifespan: 250,
      quantity: 6,
      emitting: false,
    });
    emitter.explode(6);
    this.scene.time.delayedCall(300, () => emitter.destroy());
  }

  private destroyEntitySprite(entity: any): void {
    const spriteComp = entity.getComponent("Sprite");
    if (spriteComp && spriteComp.sprite) {
      spriteComp.sprite.destroy();
    }
  }
}
