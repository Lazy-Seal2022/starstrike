import Phaser from "phaser";
import { Component } from "../Component";
import { ShipStats, UpgradeLevels } from "../../types";

export class TransformComponent implements Component {
  public readonly type = "Transform";
  constructor(
    public x: number = 0,
    public y: number = 0,
    public rotation: number = 0,
    public scale: number = 1
  ) {}
}

export class PhysicsComponent implements Component {
  public readonly type = "Physics";
  public vx: number = 0;
  public vy: number = 0;
  public isThrusting: boolean = false;

  constructor(
    public acceleration: number = 400,
    public maxSpeed: number = 300,
    public drag: number = 0.985,
    public angularVelocity: number = 4.5
  ) {}
}

export class SpriteComponent implements Component {
  public readonly type = "Sprite";
  constructor(public sprite: Phaser.GameObjects.Sprite | Phaser.GameObjects.Image) {}
}

export class HealthComponent implements Component {
  public readonly type = "Health";
  constructor(
    public current: number,
    public max: number,
    public regenRate: number = 8
  ) {}
}

export class EnergyComponent implements Component {
  public readonly type = "Energy";
  constructor(
    public current: number,
    public max: number,
    public regenRate: number = 18
  ) {}
}

export class WeaponComponent implements Component {
  public readonly type = "Weapon";
  public lastFiredTime: number = 0;

  constructor(
    public damage: number = 18,
    public pulseSpeed: number = 600,
    public fireRateMs: number = 220,
    public energyCost: number = 12
  ) {}
}

export class AsteroidComponent implements Component {
  public readonly type = "Asteroid";
  constructor(
    public size: "large" | "medium" | "small",
    public health: number,
    public maxHealth: number,
    public gemsCount: number
  ) {}
}

export class GemComponent implements Component {
  public readonly type = "Gem";
  public value: number = 1;
  public isMagnetized: boolean = false;
  constructor(value: number = 1) {
    this.value = value;
  }
}

export class LaserComponent implements Component {
  public readonly type = "Laser";
  public distanceTraveled: number = 0;
  public maxRange: number = 900;
  constructor(
    public damage: number,
    public ownerId: string,
    public isEnemy: boolean = false
  ) {}
}

export class AIComponent implements Component {
  public readonly type = "AI";
  public state: "patrol" | "chase" | "attack" = "patrol";
  public patrolOriginX: number;
  public patrolOriginY: number;
  public patrolAngle: number = 0;
  public sightRadius: number = 550;
  public attackRadius: number = 380;
  public attackCooldownMs: number = 650;
  public lastAttackTime: number = 0;

  constructor(originX: number, originY: number) {
    this.patrolOriginX = originX;
    this.patrolOriginY = originY;
    this.patrolAngle = Math.random() * Math.PI * 2;
  }
}

export class PlayerComponent implements Component {
  public readonly type = "Player";
  public gems: number = 0;
  public currentShipId: string = "scout";
  public upgradeLevels: UpgradeLevels = {
    shieldCapacity: 0,
    shieldRegen: 0,
    energyCapacity: 0,
    energyRegen: 0,
    pulseDamage: 0,
    pulseSpeed: 0,
    speed: 0,
    agility: 0,
  };

  constructor(currentShipId: string = "scout") {
    this.currentShipId = currentShipId;
  }
}
