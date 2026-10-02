import Phaser from "phaser";
import { System } from "../System";
import { World } from "../World";
import {
  TransformComponent,
  PhysicsComponent,
  AIComponent,
  WeaponComponent,
  HealthComponent,
  LaserComponent,
  SpriteComponent,
} from "../components";

export class AISystem implements System {
  constructor(private scene: Phaser.Scene) {}

  public update(world: World, delta: number): void {
    const dt = delta / 1000;
    const players = world.query("Player", "Transform", "Health");
    const playerEntity = players.length > 0 && players[0].isAlive ? players[0] : null;

    const pTransform = playerEntity ? playerEntity.getComponent<TransformComponent>("Transform") : null;
    const pHealth = playerEntity ? playerEntity.getComponent<HealthComponent>("Health") : null;
    const isPlayerAlive = pHealth && pHealth.current > 0;

    const drones = world.query("AI", "Transform", "Physics", "Weapon");
    const now = this.scene.time.now;

    for (const drone of drones) {
      if (!drone.isAlive) continue;

      const ai = drone.getComponent<AIComponent>("AI")!;
      const transform = drone.getComponent<TransformComponent>("Transform")!;
      const physics = drone.getComponent<PhysicsComponent>("Physics")!;
      const weapon = drone.getComponent<WeaponComponent>("Weapon")!;

      let targetX = ai.patrolOriginX;
      let targetY = ai.patrolOriginY;
      let shouldShoot = false;

      if (isPlayerAlive && pTransform) {
        const dx = pTransform.x - transform.x;
        const dy = pTransform.y - transform.y;
        const distToPlayer = Math.sqrt(dx * dx + dy * dy);

        if (distToPlayer < ai.sightRadius) {
          ai.state = distToPlayer < ai.attackRadius ? "attack" : "chase";
          targetX = pTransform.x;
          targetY = pTransform.y;

          // Steer towards player
          const targetAngle = Math.atan2(dy, dx);
          const angleDiff = Phaser.Math.Angle.Wrap(targetAngle - transform.rotation);
          transform.rotation += angleDiff * Math.min(1, physics.angularVelocity * dt * 2.5);

          // Thrust forward if not too close
          if (distToPlayer > 180) {
            physics.vx += Math.cos(transform.rotation) * physics.acceleration * dt * 0.9;
            physics.vy += Math.sin(transform.rotation) * physics.acceleration * dt * 0.9;
          }

          // Check if facing player and ready to fire
          if (distToPlayer < ai.attackRadius && Math.abs(angleDiff) < 0.4) {
            if (now - ai.lastAttackTime > ai.attackCooldownMs) {
              shouldShoot = true;
              ai.lastAttackTime = now;
            }
          }
        } else {
          // Return to patrol
          ai.state = "patrol";
        }
      } else {
        ai.state = "patrol";
      }

      // Patrol behavior: circular orbit around origin
      if (ai.state === "patrol") {
        ai.patrolAngle += 0.4 * dt;
        const patrolRadius = 220;
        const patrolTargetX = ai.patrolOriginX + Math.cos(ai.patrolAngle) * patrolRadius;
        const patrolTargetY = ai.patrolOriginY + Math.sin(ai.patrolAngle) * patrolRadius;

        const pAngle = Math.atan2(patrolTargetY - transform.y, patrolTargetX - transform.x);
        const pDiff = Phaser.Math.Angle.Wrap(pAngle - transform.rotation);
        transform.rotation += pDiff * Math.min(1, physics.angularVelocity * dt * 1.5);

        physics.vx += Math.cos(transform.rotation) * physics.acceleration * dt * 0.5;
        physics.vy += Math.sin(transform.rotation) * physics.acceleration * dt * 0.5;
      }

      // Fire enemy laser bolt
      if (shouldShoot) {
        this.fireEnemyLaser(world, drone.id, transform, weapon);
      }
    }
  }

  private fireEnemyLaser(
    world: World,
    ownerId: string,
    transform: TransformComponent,
    weapon: WeaponComponent
  ): void {
    const angle = transform.rotation;
    const noseX = transform.x + Math.cos(angle) * 20;
    const noseY = transform.y + Math.sin(angle) * 20;

    const laserEntity = world.createEntity();
    const sprite = this.scene.add.sprite(noseX, noseY, "laser_enemy");
    sprite.setRotation(angle);
    sprite.setDepth(9);

    laserEntity.addComponent(new TransformComponent(noseX, noseY, angle));
    laserEntity.addComponent(new PhysicsComponent(0, 1000, 1.0, 0));
    laserEntity.addComponent(new SpriteComponent(sprite));
    laserEntity.addComponent(new LaserComponent(weapon.damage, ownerId, true)); // isEnemy = true

    const lPhysics = laserEntity.getComponent<PhysicsComponent>("Physics")!;
    lPhysics.vx = Math.cos(angle) * weapon.pulseSpeed;
    lPhysics.vy = Math.sin(angle) * weapon.pulseSpeed;
  }
}
