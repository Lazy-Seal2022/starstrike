import { System } from "../System";
import { World } from "../World";
import { TransformComponent, PhysicsComponent, SpriteComponent } from "../components";
import { GAME_CONFIG } from "../../core/Config";

export class MovementSystem implements System {
  public update(world: World, delta: number): void {
    const dt = delta / 1000;
    const entities = world.query("Transform", "Physics");

    for (const entity of entities) {
      const transform = entity.getComponent<TransformComponent>("Transform")!;
      const physics = entity.getComponent<PhysicsComponent>("Physics")!;
      const spriteComp = entity.getComponent<SpriteComponent>("Sprite");

      // Apply drag
      physics.vx *= Math.pow(physics.drag, dt * 60);
      physics.vy *= Math.pow(physics.drag, dt * 60);

      // Clamp speed
      const currentSpeed = Math.sqrt(physics.vx * physics.vx + physics.vy * physics.vy);
      if (currentSpeed > physics.maxSpeed) {
        physics.vx = (physics.vx / currentSpeed) * physics.maxSpeed;
        physics.vy = (physics.vy / currentSpeed) * physics.maxSpeed;
      }

      // Update positions
      transform.x += physics.vx * dt;
      transform.y += physics.vy * dt;

      // Keep within world bounds
      const halfW = GAME_CONFIG.world.width / 2;
      const halfH = GAME_CONFIG.world.height / 2;

      if (transform.x < -halfW) {
        transform.x = -halfW;
        physics.vx *= -0.5;
      } else if (transform.x > halfW) {
        transform.x = halfW;
        physics.vx *= -0.5;
      }

      if (transform.y < -halfH) {
        transform.y = -halfH;
        physics.vy *= -0.5;
      } else if (transform.y > halfH) {
        transform.y = halfH;
        physics.vy *= -0.5;
      }

      // Synchronize Phaser Sprite if present
      if (spriteComp && spriteComp.sprite && spriteComp.sprite.active) {
        spriteComp.sprite.setPosition(transform.x, transform.y);
        spriteComp.sprite.setRotation(transform.rotation);
      }
    }
  }
}
