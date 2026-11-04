import Phaser from "phaser";

/**
 * Procedurally generates futuristic vector textures into Phaser's TextureManager.
 * This guarantees 100% self-contained assets with zero external image loading errors.
 */
export class AssetGenerator {
  public static generateAll(scene: Phaser.Scene): void {
    this.generateShipScout(scene);
    this.generateShipFighter(scene);
    this.generateShipMiner(scene);
    this.generateShipDrone(scene);
    this.generateAsteroids(scene);
    this.generateGems(scene);
    this.generateLaser(scene);
    this.generateLaserEnemy(scene);
    this.generateParticles(scene);
  }

  private static generateShipScout(scene: Phaser.Scene): void {
    if (scene.textures.exists("ship_scout")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });
    const size = 48;
    const half = size / 2;

    // Outer Hull (Triangle Arrowhead)
    g.fillStyle(0x0f172a, 1);
    g.lineStyle(2, 0x38bdf8, 1);
    g.beginPath();
    g.moveTo(size - 6, half); // Nose pointing RIGHT (0 rad in Phaser)
    g.lineTo(8, 8);
    g.lineTo(14, half);
    g.lineTo(8, size - 8);
    g.closePath();
    g.fillPath();
    g.strokePath();

    // Wing accents
    g.lineStyle(1.5, 0x0284c7, 1);
    g.beginPath();
    g.moveTo(size - 14, half);
    g.lineTo(16, 14);
    g.moveTo(size - 14, half);
    g.lineTo(16, size - 14);
    g.strokePath();

    // Cockpit Canopy (Glowing cyan)
    g.fillStyle(0x38bdf8, 0.9);
    g.fillCircle(size - 18, half, 4);

    // Engine Exhaust Ports
    g.fillStyle(0x0284c7, 1);
    g.fillRect(6, half - 6, 4, 3);
    g.fillRect(6, half + 3, 4, 3);

    g.generateTexture("ship_scout", size, size);
    g.destroy();
  }

  private static generateShipFighter(scene: Phaser.Scene): void {
    if (scene.textures.exists("ship_fighter")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });
    const size = 56;
    const half = size / 2;

    // Aggressive Swept-Wing Fighter
    g.fillStyle(0x1e1b4b, 1);
    g.lineStyle(2.5, 0xf43f5e, 1);
    g.beginPath();
    g.moveTo(size - 4, half);
    g.lineTo(10, 6);
    g.lineTo(18, half - 8);
    g.lineTo(6, half);
    g.lineTo(18, half + 8);
    g.lineTo(10, size - 6);
    g.closePath();
    g.fillPath();
    g.strokePath();

    // Dual Weapon Barrels
    g.fillStyle(0xfb7185, 1);
    g.fillRect(size - 20, 10, 8, 3);
    g.fillRect(size - 20, size - 13, 8, 3);

    // Cockpit
    g.fillStyle(0xffe4e6, 0.9);
    g.fillCircle(size - 20, half, 5);

    g.generateTexture("ship_fighter", size, size);
    g.destroy();
  }

  private static generateShipMiner(scene: Phaser.Scene): void {
    if (scene.textures.exists("ship_miner")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });
    const size = 64;
    const half = size / 2;

    // Heavy Diamond-Armored Mining Vessel
    g.fillStyle(0x1c1917, 1);
    g.lineStyle(2.5, 0xf59e0b, 1);
    g.beginPath();
    g.moveTo(size - 6, half);
    g.lineTo(size - 22, 8);
    g.lineTo(10, 14);
    g.lineTo(18, half);
    g.lineTo(10, size - 14);
    g.lineTo(size - 22, size - 8);
    g.closePath();
    g.fillPath();
    g.strokePath();

    // Heavy Mining Core
    g.fillStyle(0xfbbf24, 0.9);
    g.fillCircle(half + 4, half, 8);

    g.generateTexture("ship_miner", size, size);
    g.destroy();
  }

  private static generateAsteroids(scene: Phaser.Scene): void {
    const configs = [
      { key: "asteroid_large", size: 64, points: 10, radius: 28, color: 0x475569, stroke: 0x64748b },
      { key: "asteroid_medium", size: 40, points: 8, radius: 17, color: 0x334155, stroke: 0x475569 },
      { key: "asteroid_small", size: 24, points: 6, radius: 10, color: 0x1e293b, stroke: 0x334155 },
    ];

    configs.forEach((cfg) => {
      if (scene.textures.exists(cfg.key)) return;
      const g = scene.make.graphics({ x: 0, y: 0 });
      const half = cfg.size / 2;

      g.fillStyle(cfg.color, 1);
      g.lineStyle(2, cfg.stroke, 1);

      g.beginPath();
      for (let i = 0; i < cfg.points; i++) {
        const angle = (i / cfg.points) * Math.PI * 2;
        // Deterministic jagged offset based on index
        const variance = 0.8 + ((i * 7 + 3) % 5) * 0.08;
        const r = cfg.radius * variance;
        const px = half + Math.cos(angle) * r;
        const py = half + Math.sin(angle) * r;
        if (i === 0) g.moveTo(px, py);
        else g.lineTo(px, py);
      }
      g.closePath();
      g.fillPath();
      g.strokePath();

      // Craters
      g.fillStyle(0x1e293b, 0.7);
      g.fillCircle(half - cfg.radius * 0.3, half - cfg.radius * 0.2, cfg.radius * 0.25);
      g.fillCircle(half + cfg.radius * 0.2, half + cfg.radius * 0.3, cfg.radius * 0.18);

      g.generateTexture(cfg.key, cfg.size, cfg.size);
      g.destroy();
    });
  }

  private static generateGems(scene: Phaser.Scene): void {
    if (scene.textures.exists("gem_crystal")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });
    const size = 20;
    const half = size / 2;

    // Glowing Hexagonal Crystal
    g.fillStyle(0x06b6d4, 1);
    g.lineStyle(1.5, 0xa5f3fc, 1);

    g.beginPath();
    for (let i = 0; i < 6; i++) {
      const angle = (i / 6) * Math.PI * 2 - Math.PI / 2;
      const px = half + Math.cos(angle) * 8;
      const py = half + Math.sin(angle) * 8;
      if (i === 0) g.moveTo(px, py);
      else g.lineTo(px, py);
    }
    g.closePath();
    g.fillPath();
    g.strokePath();

    // Inner highlight
    g.fillStyle(0xffffff, 0.9);
    g.fillCircle(half - 1, half - 1, 2);

    g.generateTexture("gem_crystal", size, size);
    g.destroy();
  }

  private static generateLaser(scene: Phaser.Scene): void {
    if (scene.textures.exists("laser_pulse")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });

    // Outer plasma glow
    g.fillStyle(0x38bdf8, 0.6);
    g.fillRoundedRect(0, 2, 24, 8, 4);

    // Inner hot core
    g.fillStyle(0xffffff, 1);
    g.fillRoundedRect(4, 4, 16, 4, 2);

    g.generateTexture("laser_pulse", 24, 12);
    g.destroy();
  }

  private static generateLaserEnemy(scene: Phaser.Scene): void {
    if (scene.textures.exists("laser_enemy")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });

    // Red/crimson enemy plasma glow
    g.fillStyle(0xef4444, 0.7);
    g.fillRoundedRect(0, 2, 22, 8, 4);

    // Yellow/white hot energy core
    g.fillStyle(0xfef08a, 1);
    g.fillRoundedRect(4, 4, 14, 4, 2);

    g.generateTexture("laser_enemy", 22, 12);
    g.destroy();
  }

  private static generateShipDrone(scene: Phaser.Scene): void {
    if (scene.textures.exists("ship_drone")) return;
    const g = scene.make.graphics({ x: 0, y: 0 });
    const size = 44;
    const half = size / 2;

    // Menacing Rogue Drone (Angular red hull)
    g.fillStyle(0x450a0a, 1);
    g.lineStyle(2, 0xef4444, 1);
    g.beginPath();
    g.moveTo(size - 4, half); // Nose pointing forward
    g.lineTo(8, 6);
    g.lineTo(16, half);
    g.lineTo(8, size - 6);
    g.closePath();
    g.fillPath();
    g.strokePath();

    // Drone Red Visor Eye
    g.fillStyle(0xff0000, 1);
    g.fillCircle(size - 16, half, 3.5);

    // Wing blasters
    g.fillStyle(0xf87171, 1);
    g.fillRect(10, 8, 8, 2);
    g.fillRect(10, size - 10, 8, 2);

    g.generateTexture("ship_drone", size, size);
    g.destroy();
  }

  private static generateParticles(scene: Phaser.Scene): void {
    if (!scene.textures.exists("thrust_particle")) {
      const g = scene.make.graphics({ x: 0, y: 0 });
      g.fillStyle(0x38bdf8, 0.8);
      g.fillCircle(6, 6, 6);
      g.fillStyle(0xffffff, 1);
      g.fillCircle(6, 6, 3);
      g.generateTexture("thrust_particle", 12, 12);
      g.destroy();
    }

    if (!scene.textures.exists("spark_particle")) {
      const g = scene.make.graphics({ x: 0, y: 0 });
      g.fillStyle(0xf59e0b, 1);
      g.fillCircle(4, 4, 4);
      g.fillStyle(0xffffff, 1);
      g.fillCircle(4, 4, 2);
      g.generateTexture("spark_particle", 8, 8);
      g.destroy();
    }
  }
}
