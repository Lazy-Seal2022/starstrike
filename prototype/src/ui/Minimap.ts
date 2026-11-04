import Phaser from "phaser";
import { World } from "@/core/ecs/World";
import { TransformComponent } from "@/components";
import { GAME_CONFIG } from "@/config/game";

export class Minimap {
  private container: Phaser.GameObjects.Container;
  private radarGraphics: Phaser.GameObjects.Graphics;
  private coordText: Phaser.GameObjects.Text;
  private readonly radarSize = 120;
  private halfRadar = 60;

  constructor(
    private scene: Phaser.Scene,
    x: number,
    y: number
  ) {
    this.container = this.scene.add.container(x, y);

    // Static Radar Frame & Background
    const bg = this.scene.add.graphics();
    bg.fillStyle(0x030712, 0.85);
    bg.lineStyle(1.5, 0x0284c7, 0.9);
    bg.fillRoundedRect(0, 0, this.radarSize, this.radarSize, 8);
    bg.strokeRoundedRect(0, 0, this.radarSize, this.radarSize, 8);

    // Radar Reticle Crosshair
    bg.lineStyle(1, 0x1e293b, 0.6);
    bg.lineBetween(this.halfRadar, 4, this.halfRadar, this.radarSize - 4);
    bg.lineBetween(4, this.halfRadar, this.radarSize - 4, this.halfRadar);

    this.container.add(bg);

    // Dynamic Blip Graphics
    this.radarGraphics = this.scene.add.graphics();
    this.container.add(this.radarGraphics);

    // Coordinates Display Text
    this.coordText = this.scene.add.text(this.halfRadar, this.radarSize + 6, "POS: 0, 0", {
      fontFamily: "monospace",
      fontSize: "10px",
      color: "#94a3b8",
    });
    this.coordText.setOrigin(0.5, 0);
    this.container.add(this.coordText);
  }

  public update(world: World): void {
    this.radarGraphics.clear();

    const worldW = GAME_CONFIG.world.width;
    const worldH = GAME_CONFIG.world.height;
    const halfW = worldW / 2;
    const halfH = worldH / 2;

    const scaleX = (this.radarSize - 12) / worldW;
    const scaleY = (this.radarSize - 12) / worldH;

    // Helper: Map world (x, y) to radar local coordinates
    const toRadar = (wx: number, wy: number) => {
      const rx = 6 + (wx + halfW) * scaleX;
      const ry = 6 + (wy + halfH) * scaleY;
      return { x: rx, y: ry };
    };

    // 1. Draw World Boundary on Radar
    this.radarGraphics.lineStyle(1, 0xef4444, 0.5);
    const b1 = toRadar(-halfW, -halfH);
    const b2 = toRadar(halfW, halfH);
    this.radarGraphics.strokeRect(b1.x, b1.y, b2.x - b1.x, b2.y - b1.y);

    // 2. Draw Asteroid Blips (dim gray/cyan dots)
    const asteroids = world.query("Asteroid", "Transform");
    this.radarGraphics.fillStyle(0x64748b, 0.6);
    for (let i = 0; i < asteroids.length; i += 2) { // sampled for fast radar rendering
      const t = asteroids[i].getComponent<TransformComponent>("Transform")!;
      const r = toRadar(t.x, t.y);
      this.radarGraphics.fillRect(r.x - 1, r.y - 1, 2, 2);
    }

    // 3. Draw Hostile Drone Blips (pulsing crimson red dots)
    const drones = world.query("AI", "Transform");
    this.radarGraphics.fillStyle(0xef4444, 1);
    for (const drone of drones) {
      if (!drone.isAlive) continue;
      const t = drone.getComponent<TransformComponent>("Transform")!;
      const r = toRadar(t.x, t.y);
      this.radarGraphics.fillCircle(r.x, r.y, 2.5);
    }

    // 4. Draw Player Blip & Heading Arrow (Cyan)
    const players = world.query("Player", "Transform");
    if (players.length > 0 && players[0].isAlive) {
      const pt = players[0].getComponent<TransformComponent>("Transform")!;
      const pr = toRadar(pt.x, pt.y);

      // Player Arrow
      this.radarGraphics.fillStyle(0x38bdf8, 1);
      this.radarGraphics.lineStyle(1, 0xffffff, 1);

      const heading = pt.rotation;
      const noseX = pr.x + Math.cos(heading) * 5;
      const noseY = pr.y + Math.sin(heading) * 5;
      const leftX = pr.x + Math.cos(heading + 2.5) * 4;
      const leftY = pr.y + Math.sin(heading + 2.5) * 4;
      const rightX = pr.x + Math.cos(heading - 2.5) * 4;
      const rightY = pr.y + Math.sin(heading - 2.5) * 4;

      this.radarGraphics.beginPath();
      this.radarGraphics.moveTo(noseX, noseY);
      this.radarGraphics.lineTo(leftX, leftY);
      this.radarGraphics.lineTo(rightX, rightY);
      this.radarGraphics.closePath();
      this.radarGraphics.fillPath();

      this.coordText.setText(`X: ${Math.round(pt.x)} Y: ${Math.round(pt.y)}`);
    }
  }

  public setPosition(x: number, y: number): void {
    this.container.setPosition(x, y);
  }
}
