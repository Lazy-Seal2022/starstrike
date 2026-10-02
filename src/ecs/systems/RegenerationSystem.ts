import { System } from "../System";
import { World } from "../World";
import { HealthComponent, EnergyComponent } from "../components";
import { eventBus } from "../../core/EventBus";

export class RegenerationSystem implements System {
  public update(world: World, delta: number): void {
    const dt = delta / 1000;
    const players = world.query("Player", "Health", "Energy");

    for (const player of players) {
      const health = player.getComponent<HealthComponent>("Health")!;
      const energy = player.getComponent<EnergyComponent>("Energy")!;

      let healthChanged = false;
      let energyChanged = false;

      // Regenerate Shield
      if (health.current < health.max) {
        health.current = Math.min(health.max, health.current + health.regenRate * dt);
        healthChanged = true;
      }

      // Regenerate Energy
      if (energy.current < energy.max) {
        energy.current = Math.min(energy.max, energy.current + energy.regenRate * dt);
        energyChanged = true;
      }

      if (healthChanged) {
        eventBus.emit("shield:changed", { current: health.current, max: health.max });
      }

      if (energyChanged) {
        eventBus.emit("energy:changed", { current: energy.current, max: energy.max });
      }
    }
  }
}
