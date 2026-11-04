import Phaser from "phaser";
import { System } from "@/core/ecs/System";
import { World } from "@/core/ecs/World";
import {
  TransformComponent,
  PhysicsComponent,
  GemComponent,
  SpriteComponent,
  PlayerComponent,
  AsteroidComponent,
} from "@/components";
import { eventBus } from "@/core/EventBus";
import { GAME_CONFIG } from "@/config/game";

export class MiningSystem implements System {
  constructor(private scene: Phaser.Scene) {
    this.setupListeners();
  }

  private setupListeners(): void {
    eventBus.on("asteroid:destroyed", (data: { x: number; y: number; size: "large" | "medium" | "small" }) => {
      // Handled in main WorldManager or spawned via scene
    });
  }

  public update(world: World, delta: number): void {
    const dt = delta / 1000;
    const players = world.query("Player", "Transform");
    if (players.length === 0) return;

    const playerEntity = players[0];
    const playerTransform = playerEntity.getComponent<TransformComponent>("Transform")!;
    const playerComp = playerEntity.getComponent<PlayerComponent>("Player")!;

    const gems = world.query("Gem", "Transform", "Physics");
    const magnetRadius = GAME_CONFIG.mining.gemMagnetRadius;
    const magnetSpeed = GAME_CONFIG.mining.gemMagnetSpeed;

    for (const gemEntity of gems) {
      const gemTransform = gemEntity.getComponent<TransformComponent>("Transform")!;
      const gemPhysics = gemEntity.getComponent<PhysicsComponent>("Physics")!;
      const gemComp = gemEntity.getComponent<GemComponent>("Gem")!;

      const dx = playerTransform.x - gemTransform.x;
      const dy = playerTransform.y - gemTransform.y;
      const dist = Math.sqrt(dx * dx + dy * dy);

      // Magnetize towards player
      if (dist < magnetRadius) {
        gemComp.isMagnetized = true;
        const angle = Math.atan2(dy, dx);
        gemPhysics.vx = Math.cos(angle) * magnetSpeed;
        gemPhysics.vy = Math.sin(angle) * magnetSpeed;
      }

      // Collect gem on contact
      if (dist < 28) {
        playerComp.gems += gemComp.value;
        eventBus.emit("gem:collected", { amount: gemComp.value, total: playerComp.gems });

        // Collect flash particle
        this.createGemPickupEffect(gemTransform.x, gemTransform.y);

        // Remove gem entity
        const spriteComp = gemEntity.getComponent<SpriteComponent>("Sprite");
        if (spriteComp && spriteComp.sprite) {
          spriteComp.sprite.destroy();
        }
        world.removeEntity(gemEntity.id);
      }
    }
  }

  private createGemPickupEffect(x: number, y: number): void {
    const emitter = this.scene.add.particles(x, y, "spark_particle", {
      speed: { min: 40, max: 120 },
      scale: { start: 0.8, end: 0 },
      tint: 0x06b6d4,
      lifespan: 200,
      quantity: 4,
      emitting: false,
    });
    emitter.explode(4);
    this.scene.time.delayedCall(250, () => emitter.destroy());
  }
}
