import Phaser from "phaser";
import { AssetGenerator } from "../core/AssetGenerator";
import { soundManager } from "../core/SoundManager";

export class BootScene extends Phaser.Scene {
  constructor() {
    super({ key: "BootScene" });
  }

  public preload(): void {
    // Show quick loading text
    const { width, height } = this.scale;
    const text = this.add.text(width / 2, height / 2, "INITIALIZING STARSTRIKE...", {
      fontFamily: "monospace",
      fontSize: "20px",
      color: "#38bdf8",
    });
    text.setOrigin(0.5);

    // Generate procedural vector textures
    AssetGenerator.generateAll(this);
  }

  public create(): void {
    // Initialize procedural audio
    soundManager.init();

    // Launch main game scene and HUD overlay
    this.scene.start("GameScene");
    this.scene.launch("HUDScene");
  }
}
