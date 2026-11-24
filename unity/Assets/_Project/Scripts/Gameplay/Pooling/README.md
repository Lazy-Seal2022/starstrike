# Pooling

## Overview
High-performance zero-garbage object pooling for projectiles, VFX particles, damage numbers, and pickups.

## Components
- `PoolManager.cs`: Thread-safe, key-based prefab pool allocation and recycling.
- `PooledObjectTracker.cs`: Helper component tracking pool lifecycle and automatic return to pool.
