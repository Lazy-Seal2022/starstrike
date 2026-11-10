# AI

## Overview
Authoritative enemy behavior trees, flocking, and multi-phase boss AI.

## Interface
- `class BossBrain` [BossBrain.cs]
- `enum BossPhase` [BossBrain.cs]
- `class EnemyAI` [EnemyAI.cs]
- `enum AIState` [EnemyAI.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [BossBrain.cs]
- Mandatory component dependency enforced via `RequireComponent` [EnemyAI.cs]
- Singleton instance exclusivity maintained via `Awake` guard [BossBrain.cs]
- Singleton instance exclusivity maintained via `Awake` guard [EnemyAI.cs]

## Side Effects
- Event broadcast across decoupled game listeners [BossBrain.cs]
- Event broadcast across decoupled game listeners [EnemyAI.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [BossBrain.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [EnemyAI.cs]
- Synchronous asset loading from Resources directory [BossBrain.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

