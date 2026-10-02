import Phaser from "phaser";
import { World } from "../../ecs/World";
import { Entity } from "../../ecs/Entity";
import {
  TransformComponent,
  PhysicsComponent,
  SpriteComponent,
  AsteroidComponent,
  GemComponent,
} from "../../ecs/components";
import { GAME_CONFIG } from "../../core/Config";
import { eventBus } from "../../core/EventBus";

export class WorldManager {
  private asteroidCount: number = 0;

  constructor(
    private scene: Phaser.Scene,
    private world: World
  ) {
    this.setupListeners();
  }

  public init(): void {
    this.createStarfield();
    this.createWorldBoundaries();
    this.spawnInitialAsteroidField();
  }

  private setupListeners(): void {
    eventBus.on(
      "asteroid:destroyed",
      (data: { x: number; y: number; size: "large" | "medium" | "small" }) => {
        this.handleAsteroidDestroyed(data.x, data.y, data.size);
      }
    );
  }

  private createStarfield(): void {
    const halfW = GAME_CONFIG.world.width / 2;
    const halfH = GAME_CONFIG.world.height / 2;

    // Distant Stars (Parallax Layer 1)
    const starsLayer1 = this.scene.add.graphics();
    starsLayer1.fillStyle(0xffffff, 0.4);
    for (let i = 0; i < 400; i++) {
      const x = Phaser.Math.Between(-halfW, halfW);
      const y = Phaser.Math.Between(-halfH, halfH);
      starsLayer1.fillCircle(x, y, 1);
    }
    starsLayer1.setScrollFactor(0.2);

    // Medium Stars (Parallax Layer 2)
    const starsLayer2 = this.scene.add.graphics();
    starsLayer2.fillStyle(0x38bdf8, 0.7);
    for (let i = 0; i < 200; i++) {
      const x = Phaser.Math.Between(-halfW, halfW);
      const y = Phaser.Math.Between(-halfH, halfH);
      starsLayer2.fillCircle(x, y, 1.5);
    }
    starsLayer2.setScrollFactor(0.5);

    // Glowing Nebula Clouds (Atmospheric space feel)
    const nebula = this.scene.add.graphics();
    nebula.fillStyle(0x1e1b4b, 0.25);
    for (let i = 0; i < 8; i++) {
      const x = Phaser.Math.Between(-halfW + 400, halfW - 400);
      const y = Phaser.Math.Between(-halfH + 400, halfH - 400);
      nebula.fillCircle(x, y, Phaser.Math.Between(250, 450));
    }
    nebula.setScrollFactor(0.1);
  }

  private createWorldBoundaries(): void {
    const halfW = GAME_CONFIG.world.width / 2;
    const halfH = GAME_CONFIG.world.height / 2;

    const boundsGraphics = this.scene.add.graphics();
    boundsGraphics.lineStyle(4, 0xef4444, 0.8);
    boundsGraphics.strokeRect(-halfW, -halfH, GAME_CONFIG.world.width, GAME_CONFIG.world.height);

    // Grid markings for space navigation
    boundsGraphics.lineStyle(1, 0x1e293b, 0.4);
    for (let x = -halfW; x <= halfW; x += 400) {
      boundsGraphics.lineBetween(x, -halfH, x, halfH);
    }
    for (let y = -halfH; y <= halfH; y += 400) {
      boundsGraphics.lineBetween(-halfW, y, halfW, y);
    }
  }

  private spawnInitialAsteroidField(): void {
    const halfW = GAME_CONFIG.world.width / 2;
    const halfH = GAME_CONFIG.world.height / 2;

    for (let i = 0; i < GAME_CONFIG.mining.asteroidCount; i++) {
      // Avoid spawning right on player center
      let x = Phaser.Math.Between(-halfW + 100, halfW - 100);
      let y = Phaser.Math.Between(-halfH + 100, halfH - 100);

      if (Math.abs(x) < 250 && Math.abs(y) < 250) {
        x += x >= 0 ? 300 : -300;
        y += y >= 0 ? 300 : -300;
      }

      const roll = Math.random();
      const size: "large" | "medium" | "small" =
        roll < 0.35 ? "large" : roll < 0.75 ? "medium" : "small";

      this.spawnAsteroid(x, y, size);
    }
  }

  public spawnAsteroid(x: number, y: number, size: "large" | "medium" | "small"): Entity {
    const entity = this.world.createEntity(`asteroid_${++this.asteroidCount}`);
    const key = `asteroid_${size}`;
    const sprite = this.scene.add.sprite(x, y, key);

    const rotation = Math.random() * Math.PI * 2;
    const speed = Phaser.Math.Between(5, 25);
    const moveAngle = Math.random() * Math.PI * 2;

    const health = GAME_CONFIG.mining.asteroidHealth[size];
    const gems = GAME_CONFIG.mining.asteroidGems[size];

    entity.addComponent(new TransformComponent(x, y, rotation));
    entity.addComponent(new PhysicsComponent(0, 50, 0.999, (Math.random() - 0.5) * 0.5));
    entity.addComponent(new SpriteComponent(sprite));
    entity.addComponent(new AsteroidComponent(size, health, health, gems));

    // Slow drift velocity
    const physics = entity.getComponent<PhysicsComponent>("Physics")!;
    physics.vx = Math.cos(moveAngle) * speed;
    physics.vy = Math.sin(moveAngle) * speed;

    return entity;
  }

  public handleAsteroidDestroyed(x: number, y: number, size: "large" | "medium" | "small"): void {
    // 1. Explosion particles
    this.createAsteroidExplosion(x, y, size);

    // 2. Drop Gems
    const gemsToDrop = GAME_CONFIG.mining.asteroidGems[size];
    for (let i = 0; i < gemsToDrop; i++) {
      this.spawnGem(x, y);
    }

    // 3. Split into smaller asteroids
    if (size === "large") {
      this.spawnAsteroid(x + Phaser.Math.Between(-15, 15), y + Phaser.Math.Between(-15, 15), "medium");
      this.spawnAsteroid(x + Phaser.Math.Between(-15, 15), y + Phaser.Math.Between(-15, 15), "medium");
    } else if (size === "medium") {
      this.spawnAsteroid(x + Phaser.Math.Between(-10, 10), y + Phaser.Math.Between(-10, 10), "small");
      this.spawnAsteroid(x + Phaser.Math.Between(-10, 10), y + Phaser.Math.Between(-10, 10), "small");
    }
  }

  private spawnGem(x: number, y: number): void {
    const gemEntity = this.world.createEntity();
    const sprite = this.scene.add.sprite(x, y, "gem_crystal");

    const angle = Math.random() * Math.PI * 2;
    const speed = Phaser.Math.Between(40, 120);

    gemEntity.addComponent(new TransformComponent(x, y, 0));
    gemEntity.addComponent(new PhysicsComponent(0, 400, 0.95, 0));
    gemEntity.addComponent(new SpriteComponent(sprite));
    gemEntity.addComponent(new GemComponent(1));

    const physics = gemEntity.getComponent<PhysicsComponent>("Physics")!;
    physics.vx = Math.cos(angle) * speed;
    physics.vy = Math.sin(angle) * speed;
  }

  private createAsteroidExplosion(x: number, y: number, size: "large" | "medium" | "small"): void {
    const count = size === "large" ? 16 : size === "medium" ? 10 : 6;
    const emitter = this.scene.add.particles(x, y, "spark_particle", {
      speed: { min: 40, max: size === "large" ? 220 : 140 },
      angle: { min: 0, max: 360 },
      scale: { start: 1.2, end: 0 },
      lifespan: 400,
      quantity: count,
      emitting: false,
    });
    emitter.explode(count);
    this.scene.time.delayedCall(500, () => emitter.destroy());
  }
}
