import Phaser from "phaser";
import { BootScene } from "./scenes/BootScene";
import { GameScene } from "./scenes/GameScene";
import { HUDScene } from "./scenes/HUDScene";

const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  parent: "game-container",
  backgroundColor: "#030712",
  scale: {
    mode: Phaser.Scale.RESIZE,
    autoCenter: Phaser.Scale.CENTER_BOTH,
    width: window.innerWidth,
    height: window.innerHeight,
  },
  fps: {
    target: 60,
    forceSetTimeOut: true,
  },
  scene: [BootScene, GameScene, HUDScene],
};

// Bootstrap the game
window.addEventListener("DOMContentLoaded", () => {
  new Phaser.Game(config);
});
