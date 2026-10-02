import Phaser from "phaser";
import { World } from "../ecs/World";
import { Entity } from "../ecs/Entity";
import {
  TransformComponent,
  PhysicsComponent,
  SpriteComponent,
  HealthComponent,
  EnergyComponent,
  WeaponComponent,
  PlayerComponent,
  LaserComponent,
} from "../ecs/components";
import { MovementSystem } from "../ecs/systems/MovementSystem";
import { CombatSystem } from "../ecs/systems/CombatSystem";
import { MiningSystem } from "../ecs/systems/MiningSystem";
import { RegenerationSystem } from "../ecs/systems/RegenerationSystem";
import { AISystem } from "../ecs/systems/AISystem";
import { WorldManager } from "../modules/world/WorldManager";
import { GAME_CONFIG, SHIPS_REGISTRY } from "../core/Config";
import { eventBus } from "../core/EventBus";
import { soundManager } from "../core/SoundManager";

export class GameScene extends Phaser.Scene {
  public ecsWorld!: World;
  public playerEntity!: Entity;
  public worldManager!: WorldManager;

  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private keyW!: Phaser.Input.Keyboard.Key;
  private keyA!: Phaser.Input.Keyboard.Key;
  private keyS!: Phaser.Input.Keyboard.Key;
  private keyD!: Phaser.Input.Keyboard.Key;
  private keySpace!: Phaser.Input.Keyboard.Key;
  private keyShift!: Phaser.Input.Keyboard.Key;

  private thrustEmitter!: Phaser.GameObjects.Particles.ParticleEmitter;

  // Touch virtual controls state
  public touchThrust: boolean = false;
  public touchFire: boolean = false;
  public touchSteerAngle: number | null = null;

  constructor() {
    super({ key: "GameScene" });
  }

  public create(): void {
    // 1. Initialize ECS World
    this.ecsWorld = new World();
    this.ecsWorld.addSystem(new MovementSystem());
    this.ecsWorld.addSystem(new AISystem(this));
    this.ecsWorld.addSystem(new CombatSystem(this));
    this.ecsWorld.addSystem(new MiningSystem(this));
    this.ecsWorld.addSystem(new RegenerationSystem());

    // 2. Initialize World Environment
    this.worldManager = new WorldManager(this, this.ecsWorld);
    this.worldManager.init();

    // 3. Spawn Player
    this.spawnPlayer();

    // 4. Setup Camera
    const playerSprite = this.playerEntity.getComponent<SpriteComponent>("Sprite")!.sprite;
    this.cameras.main.startFollow(playerSprite, true, 0.08, 0.08);
    this.cameras.main.setZoom(1.0);

    const halfW = GAME_CONFIG.world.width / 2;
    const halfH = GAME_CONFIG.world.height / 2;
    this.cameras.main.setBounds(-halfW, -halfH, GAME_CONFIG.world.width, GAME_CONFIG.world.height);

    // 5. Setup Keyboard Inputs
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.keyW = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.W);
      this.keyA = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.A);
      this.keyS = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.S);
      this.keyD = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.D);
      this.keySpace = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);
      this.keyShift = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SHIFT);
    }

    // 6. Thrust Particle Emitter
    this.thrustEmitter = this.add.particles(0, 0, "thrust_particle", {
      speed: { min: 60, max: 120 },
      scale: { start: 0.8, end: 0 },
      lifespan: 180,
      quantity: 2,
      emitting: false,
    });

    // 7. Setup Mouse Click / Aiming
    this.input.on("pointerdown", (pointer: Phaser.Input.Pointer) => {
      if (pointer.leftButtonDown()) {
        this.fireWeapon();
      }
    });

    // Notify spawn
    eventBus.emit("player:spawned", { x: 0, y: 0 });
  }

  private spawnPlayer(): void {
    const shipDef = SHIPS_REGISTRY["scout"];
    this.playerEntity = this.ecsWorld.createEntity("player");

    const sprite = this.add.sprite(0, 0, shipDef.spriteKey);
    sprite.setDepth(10);

    this.playerEntity.addComponent(new TransformComponent(0, 0, 0, shipDef.scale));
    this.playerEntity.addComponent(
      new PhysicsComponent(
        shipDef.baseStats.speed * 1.5,
        shipDef.baseStats.speed,
        GAME_CONFIG.physics.baseFriction,
        shipDef.baseStats.agility
      )
    );
    this.playerEntity.addComponent(new SpriteComponent(sprite));
    this.playerEntity.addComponent(
      new HealthComponent(shipDef.baseStats.shieldCapacity, shipDef.baseStats.shieldCapacity, shipDef.baseStats.shieldRegen)
    );
    this.playerEntity.addComponent(
      new EnergyComponent(shipDef.baseStats.energyCapacity, shipDef.baseStats.energyCapacity, shipDef.baseStats.energyRegen)
    );
    this.playerEntity.addComponent(
      new WeaponComponent(shipDef.baseStats.pulseDamage, shipDef.baseStats.pulseSpeed, 200, 12)
    );
    this.playerEntity.addComponent(new PlayerComponent(shipDef.id));
  }

  public update(time: number, delta: number): void {
    if (!this.playerEntity || !this.playerEntity.isAlive) return;

    this.handlePlayerInput(time, delta);
    this.ecsWorld.update(delta);
  }

  private handlePlayerInput(time: number, delta: number): void {
    const dt = delta / 1000;
    const transform = this.playerEntity.getComponent<TransformComponent>("Transform")!;
    const physics = this.playerEntity.getComponent<PhysicsComponent>("Physics")!;
    const energy = this.playerEntity.getComponent<EnergyComponent>("Energy")!;

    // 1. Aiming / Rotation (Mouse aim takes priority on desktop, touch steer on mobile)
    if (this.touchSteerAngle !== null) {
      transform.rotation = this.touchSteerAngle;
    } else {
      const pointer = this.input.activePointer;
      const worldPoint = this.cameras.main.getWorldPoint(pointer.x, pointer.y);
      const targetAngle = Math.atan2(worldPoint.y - transform.y, worldPoint.x - transform.x);

      // Smooth rotation towards cursor
      const diff = Phaser.Math.Angle.Wrap(targetAngle - transform.rotation);
      transform.rotation += diff * Math.min(1, physics.angularVelocity * dt * 3);
    }

    // 2. Thrust & Boost
    const isW = this.keyW?.isDown || this.cursors?.up?.isDown || this.touchThrust;
    const isS = this.keyS?.isDown || this.cursors?.down?.isDown;
    const isBoost = (this.keyShift?.isDown || this.input.activePointer.rightButtonDown()) && energy.current > 10;

    let accel = physics.acceleration;
    if (isBoost) {
      accel *= 1.8;
      energy.current = Math.max(0, energy.current - 25 * dt);
      eventBus.emit("energy:changed", { current: energy.current, max: energy.max });
    }

    physics.isThrusting = isW;

    if (isW) {
      physics.vx += Math.cos(transform.rotation) * accel * dt;
      physics.vy += Math.sin(transform.rotation) * accel * dt;

      // Exhaust particles from rear
      const rearX = transform.x - Math.cos(transform.rotation) * 20;
      const rearY = transform.y - Math.sin(transform.rotation) * 20;
      this.thrustEmitter.setPosition(rearX, rearY);
      this.thrustEmitter.particleAngle = { min: (transform.rotation + Math.PI - 0.4) * (180 / Math.PI), max: (transform.rotation + Math.PI + 0.4) * (180 / Math.PI) };
      this.thrustEmitter.explode(2);
    } else if (isS) {
      // Reverse / Brake
      physics.vx -= Math.cos(transform.rotation) * (accel * 0.4) * dt;
      physics.vy -= Math.sin(transform.rotation) * (accel * 0.4) * dt;
    }

    // 3. Firing (Continuous while holding Space or Left Mouse or Touch Fire)
    const isFiring =
      this.keySpace?.isDown ||
      this.input.activePointer.leftButtonDown() ||
      this.touchFire;

    if (isFiring) {
      this.fireWeapon();
    }
  }

  public fireWeapon(): void {
    if (!this.playerEntity || !this.playerEntity.isAlive) return;

    const weapon = this.playerEntity.getComponent<WeaponComponent>("Weapon")!;
    const energy = this.playerEntity.getComponent<EnergyComponent>("Energy")!;
    const transform = this.playerEntity.getComponent<TransformComponent>("Transform")!;
    const physics = this.playerEntity.getComponent<PhysicsComponent>("Physics")!;
    const playerComp = this.playerEntity.getComponent<PlayerComponent>("Player")!;

    const now = this.time.now;
    if (now - weapon.lastFiredTime < weapon.fireRateMs) return;
    if (energy.current < weapon.energyCost) return;

    // Deduct energy
    energy.current -= weapon.energyCost;
    weapon.lastFiredTime = now;
    eventBus.emit("energy:changed", { current: energy.current, max: energy.max });
    soundManager.playLaser();

    // Slight weapon recoil
    physics.vx -= Math.cos(transform.rotation) * 15;
    physics.vy -= Math.sin(transform.rotation) * 15;

    // Spawn Lasers (1 or 2 barrels depending on ship tier)
    const shipDef = SHIPS_REGISTRY[playerComp.currentShipId];
    const barrelOffsets = shipDef && shipDef.weaponSlots === 2 ? [-12, 12] : [0];

    for (const offset of barrelOffsets) {
      const angle = transform.rotation;
      const noseX = transform.x + Math.cos(angle) * 24 - Math.sin(angle) * offset;
      const noseY = transform.y + Math.sin(angle) * 24 + Math.cos(angle) * offset;

      const laserEntity = this.ecsWorld.createEntity();
      const sprite = this.add.sprite(noseX, noseY, "laser_pulse");
      sprite.setRotation(angle);
      sprite.setDepth(9);

      laserEntity.addComponent(new TransformComponent(noseX, noseY, angle));
      laserEntity.addComponent(new PhysicsComponent(0, 1200, 1.0, 0));
      laserEntity.addComponent(new SpriteComponent(sprite));
      laserEntity.addComponent(new LaserComponent(weapon.damage, this.playerEntity.id));

      const lPhysics = laserEntity.getComponent<PhysicsComponent>("Physics")!;
      lPhysics.vx = Math.cos(angle) * weapon.pulseSpeed + physics.vx * 0.3;
      lPhysics.vy = Math.sin(angle) * weapon.pulseSpeed + physics.vy * 0.3;
    }
  }
}
