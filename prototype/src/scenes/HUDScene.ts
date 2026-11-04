import Phaser from "phaser";
import { eventBus } from "@/core/EventBus";
import { UpgradeManager } from "@/features/progression/UpgradeManager";
import { ShipEvolution } from "@/features/progression/ShipEvolution";
import { SHIPS_REGISTRY } from "@/config/ships";
import { UpgradeKey } from "@/core/types";
import { GameScene } from "@/scenes/GameScene";
import { Minimap } from "@/ui/Minimap";

export class HUDScene extends Phaser.Scene {
  private shieldBar!: Phaser.GameObjects.Graphics;
  private energyBar!: Phaser.GameObjects.Graphics;
  private gemText!: Phaser.GameObjects.Text;
  private shipNameText!: Phaser.GameObjects.Text;
  private minimap!: Minimap;

  private upgradeContainer!: Phaser.GameObjects.Container;
  private upgradeButtons: Map<UpgradeKey, { btn: Phaser.GameObjects.Text; costText: Phaser.GameObjects.Text }> = new Map();
  private evolutionContainer!: Phaser.GameObjects.Container;

  private isUpgradeOpen: boolean = false;

  constructor() {
    super({ key: "HUDScene" });
  }

  public create(): void {
    const { width, height } = this.scale;

    // 1. Top HUD Bar
    this.createTopHUD();

    // 2. Upgrade Panel (bottom left)
    this.createUpgradePanel();

    // 3. Evolution Modal
    this.createEvolutionModal();

    // 4. Mobile Touch Controls
    this.createTouchControls();

    // 5. Setup Event Listeners
    this.setupEventListeners();

    // 6. Tactical Radar Minimap (Top Right)
    this.minimap = new Minimap(this, width - 136, 12);

    // 7. Keyboard shortcut for Upgrade Panel
    if (this.input.keyboard) {
      this.input.keyboard.on("keydown-U", () => {
        this.toggleUpgradePanel();
      });
    }

    // Handle Resize
    this.scale.on("resize", (gameSize: Phaser.Structs.Size) => {
      this.handleResize(gameSize.width, gameSize.height);
    });
  }

  public update(): void {
    const gameScene = this.scene.get("GameScene") as GameScene;
    if (gameScene && gameScene.ecsWorld && this.minimap) {
      this.minimap.update(gameScene.ecsWorld);
    }
  }

  private createTopHUD(): void {
    // Background bar
    const bg = this.add.graphics();
    bg.fillStyle(0x030712, 0.75);
    bg.fillRoundedRect(16, 12, 540, 52, 10);
    bg.lineStyle(1.5, 0x1e293b, 1);
    bg.strokeRoundedRect(16, 12, 540, 52, 10);

    // Ship Info
    this.shipNameText = this.add.text(28, 20, "T1 • DELTA SCOUT", {
      fontFamily: "monospace",
      fontSize: "13px",
      fontStyle: "bold",
      color: "#38bdf8",
    });

    // Shield Bar
    this.shieldBar = this.add.graphics();
    this.updateShieldBar(100, 100);

    const shieldLabel = this.add.text(180, 21, "SHIELD", {
      fontFamily: "monospace",
      fontSize: "11px",
      color: "#94a3b8",
    });

    // Energy Bar
    this.energyBar = this.add.graphics();
    this.updateEnergyBar(80, 80);

    const energyLabel = this.add.text(320, 21, "ENERGY", {
      fontFamily: "monospace",
      fontSize: "11px",
      color: "#94a3b8",
    });

    // Gem / Crystal Counter
    this.gemText = this.add.text(450, 26, "💎 0", {
      fontFamily: "monospace",
      fontSize: "16px",
      fontStyle: "bold",
      color: "#06b6d4",
    });

    // Upgrade Toggle Button
    const upgradeToggleBtn = this.add.text(widthSafe(this.scale.width, 140), 22, "[U] UPGRADES", {
      fontFamily: "monospace",
      fontSize: "13px",
      fontStyle: "bold",
      color: "#f59e0b",
      backgroundColor: "#1e1b4b",
      padding: { x: 10, y: 6 },
    });
    upgradeToggleBtn.setInteractive({ useHandCursor: true });
    upgradeToggleBtn.on("pointerdown", () => this.toggleUpgradePanel());
  }

  private updateShieldBar(current: number, max: number): void {
    this.shieldBar.clear();
    const pct = Math.max(0, Math.min(1, current / max));
    const width = 110;

    // Track
    this.shieldBar.fillStyle(0x1e293b, 0.9);
    this.shieldBar.fillRoundedRect(180, 36, width, 14, 4);

    // Fill
    this.shieldBar.fillStyle(0x38bdf8, 1);
    this.shieldBar.fillRoundedRect(180, 36, width * pct, 14, 4);
  }

  private updateEnergyBar(current: number, max: number): void {
    this.energyBar.clear();
    const pct = Math.max(0, Math.min(1, current / max));
    const width = 110;

    // Track
    this.energyBar.fillStyle(0x1e293b, 0.9);
    this.energyBar.fillRoundedRect(320, 36, width, 14, 4);

    // Fill
    this.energyBar.fillStyle(0xf59e0b, 1);
    this.energyBar.fillRoundedRect(320, 36, width * pct, 14, 4);
  }

  private createUpgradePanel(): void {
    const { height } = this.scale;
    this.upgradeContainer = this.add.container(20, height - 320);

    const bg = this.add.graphics();
    bg.fillStyle(0x090d16, 0.92);
    bg.lineStyle(1.5, 0x0284c7, 0.8);
    bg.fillRoundedRect(0, 0, 310, 300, 12);
    bg.strokeRoundedRect(0, 0, 310, 300, 12);
    this.upgradeContainer.add(bg);

    const title = this.add.text(14, 12, "⚙️ SHIP SYSTEMS UPGRADE", {
      fontFamily: "monospace",
      fontSize: "14px",
      fontStyle: "bold",
      color: "#38bdf8",
    });
    this.upgradeContainer.add(title);

    const stats: { key: UpgradeKey; label: string }[] = [
      { key: "shieldCapacity", label: "Shield Cap" },
      { key: "shieldRegen", label: "Shield Regen" },
      { key: "energyCapacity", label: "Energy Cap" },
      { key: "energyRegen", label: "Energy Regen" },
      { key: "pulseDamage", label: "Pulse Damage" },
      { key: "pulseSpeed", label: "Pulse Speed" },
      { key: "speed", label: "Top Speed" },
      { key: "agility", label: "Agility" },
    ];

    let startY = 44;
    stats.forEach((stat) => {
      const label = this.add.text(16, startY, stat.label, {
        fontFamily: "monospace",
        fontSize: "12px",
        color: "#cbd5e1",
      });
      this.upgradeContainer.add(label);

      const costText = this.add.text(150, startY, "10 💎", {
        fontFamily: "monospace",
        fontSize: "11px",
        color: "#06b6d4",
      });
      this.upgradeContainer.add(costText);

      const btn = this.add.text(230, startY - 2, "[+ UP]", {
        fontFamily: "monospace",
        fontSize: "11px",
        fontStyle: "bold",
        color: "#ffffff",
        backgroundColor: "#0284c7",
        padding: { x: 8, y: 3 },
      });
      btn.setInteractive({ useHandCursor: true });
      btn.on("pointerdown", () => this.tryUpgrade(stat.key));
      this.upgradeContainer.add(btn);

      this.upgradeButtons.set(stat.key, { btn, costText });
      startY += 30;
    });

    // Start open for player discovery
    this.isUpgradeOpen = true;
  }

  private tryUpgrade(key: UpgradeKey): void {
    const gameScene = this.scene.get("GameScene") as GameScene;
    if (!gameScene || !gameScene.playerEntity) return;

    const success = UpgradeManager.upgrade(gameScene.playerEntity, key);
    if (success) {
      this.refreshUpgradeButtons();
      this.checkEvolutionStatus();
    }
  }

  private refreshUpgradeButtons(): void {
    const gameScene = this.scene.get("GameScene") as GameScene;
    if (!gameScene || !gameScene.playerEntity) return;

    const playerComp = gameScene.playerEntity.getComponent<any>("Player");
    if (!playerComp) return;

    const shipDef = SHIPS_REGISTRY[playerComp.currentShipId];
    if (!shipDef) return;

    this.upgradeButtons.forEach(({ btn, costText }, key) => {
      const lvl = playerComp.upgradeLevels[key];
      const maxLvl = shipDef.maxUpgradeLevel;

      if (lvl >= maxLvl) {
        costText.setText("MAXED");
        costText.setColor("#10b981");
        btn.setText("[MAX]");
        btn.setBackgroundColor("#334155");
        btn.disableInteractive();
      } else {
        const cost = UpgradeManager.getUpgradeCost(lvl);
        costText.setText(`${cost} 💎 (L${lvl})`);
        costText.setColor(playerComp.gems >= cost ? "#06b6d4" : "#64748b");

        btn.setText("[+ UP]");
        if (playerComp.gems >= cost) {
          btn.setBackgroundColor("#0284c7");
          btn.setInteractive({ useHandCursor: true });
        } else {
          btn.setBackgroundColor("#1e293b");
        }
      }
    });
  }

  private toggleUpgradePanel(): void {
    this.isUpgradeOpen = !this.isUpgradeOpen;
    this.upgradeContainer.setVisible(this.isUpgradeOpen);
    if (this.isUpgradeOpen) {
      this.refreshUpgradeButtons();
    }
  }

  private createEvolutionModal(): void {
    const { width, height } = this.scale;
    this.evolutionContainer = this.add.container(width / 2 - 200, height / 2 - 120);

    const bg = this.add.graphics();
    bg.fillStyle(0x0f172a, 0.95);
    bg.lineStyle(2, 0x10b981, 1);
    bg.fillRoundedRect(0, 0, 400, 240, 16);
    bg.strokeRoundedRect(0, 0, 400, 240, 16);
    this.evolutionContainer.add(bg);

    const title = this.add.text(200, 30, "⚡ SHIP EVOLUTION READY! ⚡", {
      fontFamily: "monospace",
      fontSize: "18px",
      fontStyle: "bold",
      color: "#10b981",
    });
    title.setOrigin(0.5);
    this.evolutionContainer.add(title);

    const sub = this.add.text(200, 65, "Select your Tier 2 advanced class:", {
      fontFamily: "monospace",
      fontSize: "13px",
      color: "#94a3b8",
    });
    sub.setOrigin(0.5);
    this.evolutionContainer.add(sub);

    // Option 1: Fighter
    const btnFighter = this.add.text(110, 130, "⚔️ ARES FIGHTER\n(Dual Blasters & Speed)", {
      fontFamily: "monospace",
      fontSize: "12px",
      fontStyle: "bold",
      color: "#ffffff",
      backgroundColor: "#f43f5e",
      align: "center",
      padding: { x: 12, y: 10 },
    });
    btnFighter.setOrigin(0.5);
    btnFighter.setInteractive({ useHandCursor: true });
    btnFighter.on("pointerdown", () => this.selectEvolution("fighter"));
    this.evolutionContainer.add(btnFighter);

    // Option 2: Miner
    const btnMiner = this.add.text(290, 130, "⛏️ GOLIATH MINER\n(Heavy Armor & Cannon)", {
      fontFamily: "monospace",
      fontSize: "12px",
      fontStyle: "bold",
      color: "#ffffff",
      backgroundColor: "#f59e0b",
      align: "center",
      padding: { x: 12, y: 10 },
    });
    btnMiner.setOrigin(0.5);
    btnMiner.setInteractive({ useHandCursor: true });
    btnMiner.on("pointerdown", () => this.selectEvolution("miner"));
    this.evolutionContainer.add(btnMiner);

    this.evolutionContainer.setVisible(false);
  }

  private checkEvolutionStatus(): void {
    const gameScene = this.scene.get("GameScene") as GameScene;
    if (!gameScene || !gameScene.playerEntity) return;

    if (ShipEvolution.canEvolve(gameScene.playerEntity)) {
      this.evolutionContainer.setVisible(true);
    }
  }

  private selectEvolution(shipId: string): void {
    const gameScene = this.scene.get("GameScene") as GameScene;
    if (!gameScene || !gameScene.playerEntity) return;

    ShipEvolution.evolve(gameScene.playerEntity, shipId);
    this.evolutionContainer.setVisible(false);

    const shipDef = SHIPS_REGISTRY[shipId];
    this.shipNameText.setText(`T${shipDef.tier} • ${shipDef.name.toUpperCase()}`);
    this.shipNameText.setColor(shipId === "fighter" ? "#f43f5e" : "#f59e0b");

    this.refreshUpgradeButtons();
  }

  private createTouchControls(): void {
    // Only show virtual touch buttons on touch-capable devices or narrow screens
    const isTouch = this.sys.game.device.input.touch || window.innerWidth < 800;
    if (!isTouch) return;

    const { width, height } = this.scale;
    const gameScene = this.scene.get("GameScene") as GameScene;

    // Fire Button (Right bottom)
    const fireBtn = this.add.circle(width - 80, height - 80, 44, 0xf43f5e, 0.7);
    fireBtn.setStrokeStyle(3, 0xffffff, 0.9);
    const fireLabel = this.add.text(width - 80, height - 80, "FIRE", {
      fontFamily: "monospace",
      fontSize: "14px",
      fontStyle: "bold",
      color: "#ffffff",
    }).setOrigin(0.5);

    fireBtn.setInteractive();
    fireBtn.on("pointerdown", () => {
      if (gameScene) gameScene.touchFire = true;
    });
    fireBtn.on("pointerup", () => {
      if (gameScene) gameScene.touchFire = false;
    });
    fireBtn.on("pointerout", () => {
      if (gameScene) gameScene.touchFire = false;
    });

    // Thrust Button (Right bottom next to fire)
    const thrustBtn = this.add.circle(width - 180, height - 80, 36, 0x38bdf8, 0.7);
    thrustBtn.setStrokeStyle(2, 0xffffff, 0.9);
    this.add.text(width - 180, height - 80, "THRUST", {
      fontFamily: "monospace",
      fontSize: "11px",
      fontStyle: "bold",
      color: "#ffffff",
    }).setOrigin(0.5);

    thrustBtn.setInteractive();
    thrustBtn.on("pointerdown", () => {
      if (gameScene) gameScene.touchThrust = true;
    });
    thrustBtn.on("pointerup", () => {
      if (gameScene) gameScene.touchThrust = false;
    });
    thrustBtn.on("pointerout", () => {
      if (gameScene) gameScene.touchThrust = false;
    });
  }

  private setupEventListeners(): void {
    eventBus.on("shield:changed", (data: { current: number; max: number }) => {
      this.updateShieldBar(data.current, data.max);
    });

    eventBus.on("energy:changed", (data: { current: number; max: number }) => {
      this.updateEnergyBar(data.current, data.max);
    });

    eventBus.on("gem:collected", (data: { amount: number; total: number }) => {
      this.gemText.setText(`💎 ${data.total}`);
      this.refreshUpgradeButtons();
    });
  }

  private handleResize(width: number, height: number): void {
    if (this.upgradeContainer) {
      this.upgradeContainer.setPosition(20, height - 320);
    }
    if (this.evolutionContainer) {
      this.evolutionContainer.setPosition(width / 2 - 200, height / 2 - 120);
    }
    if (this.minimap) {
      this.minimap.setPosition(width - 136, 12);
    }
  }
}

function widthSafe(w: number, offset: number): number {
  return Math.max(580, Math.min(w - offset, 800));
}
