# Managers

## Overview
Game session lifecycle orchestration, global state management, world boundary supervision, and enemy wave pacing.

## Components
- `GameManager.cs`: Primary match state machine (Menu, Playing, Paused, GameOver, Victory).
- `WorldManager.cs`: Sector world boundary, coordinate wrapping/clamping, and spatial anchor.
- `WaveManager.cs`: Spawning director for progressive enemy waves and boss encounters.
