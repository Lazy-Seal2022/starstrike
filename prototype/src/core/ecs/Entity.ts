import { Component } from "@/core/ecs/Component";

export class Entity {
  public readonly id: string;
  private components: Map<string, Component> = new Map();
  public isAlive: boolean = true;

  constructor(id?: string) {
    this.id = id || `entity_${Math.random().toString(36).substring(2, 9)}`;
  }

  public addComponent<T extends Component>(component: T): this {
    this.components.set(component.type, component);
    return this;
  }

  public getComponent<T extends Component>(type: string): T | undefined {
    return this.components.get(type) as T | undefined;
  }

  public hasComponent(type: string): boolean {
    return this.components.has(type);
  }

  public removeComponent(type: string): this {
    this.components.delete(type);
    return this;
  }

  public destroy(): void {
    this.isAlive = false;
    this.components.clear();
  }
}
