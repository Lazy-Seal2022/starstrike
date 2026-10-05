import { Entity } from "@/core/ecs/Entity";
import { System } from "@/core/ecs/System";

export class World {
  private entities: Map<string, Entity> = new Map();
  private systems: System[] = [];

  public createEntity(id?: string): Entity {
    const entity = new Entity(id);
    this.entities.set(entity.id, entity);
    return entity;
  }

  public addEntity(entity: Entity): this {
    this.entities.set(entity.id, entity);
    return this;
  }

  public removeEntity(id: string): void {
    const entity = this.entities.get(id);
    if (entity) {
      entity.destroy();
      this.entities.delete(id);
    }
  }

  public getEntity(id: string): Entity | undefined {
    return this.entities.get(id);
  }

  public getAllEntities(): Entity[] {
    return Array.from(this.entities.values());
  }

  public query(...componentTypes: string[]): Entity[] {
    return this.getAllEntities().filter((e) =>
      e.isAlive && componentTypes.every((type) => e.hasComponent(type))
    );
  }

  public addSystem(system: System): this {
    this.systems.push(system);
    return this;
  }

  public update(delta: number): void {
    // Run all systems in sequence
    for (const system of this.systems) {
      system.update(this, delta);
    }

    // Clean up destroyed entities
    for (const [id, entity] of this.entities.entries()) {
      if (!entity.isAlive) {
        this.entities.delete(id);
      }
    }
  }

  public clear(): void {
    this.entities.forEach((e) => e.destroy());
    this.entities.clear();
    this.systems = [];
  }
}
