import Phaser from "phaser";
import { System } from "../System";
import { World } from "../World";
import {
  TransformComponent,
  PhysicsComponent,
  LaserComponent,
  AsteroidComponent,
  SpriteComponent,
} from "../components";
import { eventBus } from "../../core/EventBus";

export class CombatSystem implements System {
  constructor(private scene: Phaser.Scene) {}

  public update(world: World, delta: number): void {
    const dt = delta / 1000;
    const lasers = world.query("Laser", "Transform", "Physics");
    const asteroids = world.query("Asteroid", "Transform");

    for (const laserEntity of lasers) {
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

      // Check collision with asteroids
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
          // Impact occurred!
          this.createImpactSparks(laserTransform.x, laserTransform.y);

          // Apply damage to asteroid
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
          break; // Laser can only hit one target
        }
      }
    }
  }

  private createImpactSparks(x: number, y: number): void {
    const emitter = this.scene.add.particles(x, y, "spark_particle", {
      speed: { min: 60, max: 180 },
      angle: { min: 0, max: 360 },
      scale: { start: 1, end: 0 },
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
