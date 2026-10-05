import { World } from "@/core/ecs/World";

export interface System {
  update(world: World, delta: number): void;
}
