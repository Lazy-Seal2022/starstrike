# Game

## Overview
Game-wide session parameters, active game mode enumerations, run performance summaries, scoring algorithms, and UI theme styling tokens.

## Interface
- `class GameConfig` [GameConfig.cs]
  - `startShield: float`
  - `startEnergy: float`
  - `defaultMode: GameMode`
  - `arenaWidth: float`
  - `arenaHeight: float`
- `enum GameMode` [GameMode.cs]
  - `Normal`, `Hardcore`, `Endless`
- `struct RunSummary` [RunSummary.cs]
  - `gemsCollected: int`
  - `asteroidsDestroyed: int`
  - `enemiesDestroyed: int`
  - `bossDefeated: bool`
  - `survivalTimeSeconds: float`
  - `score: int`
  - `CalculateScore(int gems, int asteroids, int enemies, bool bossKilled, float time) -> int`
- `class UIThemeConfig` [UIThemeConfig.cs]
  - Color palettes, typography styles, and HUD padding configurations

## Invariants
- `CalculateScore` guarantees non-negative score values.
- Arena bounds maintain non-negative widths and heights.

## Side Effects
- Score calculations are purely functional.
- Configuration instances are read-only.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
