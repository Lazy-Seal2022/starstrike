# prototype

## Overview
StarStrike web prototype project root and build tooling.

## Interface
- `default (ViteUserConfig)` [vite.config.ts] — Vite build configuration: base `"./"`, `@` alias to `src/`, Phaser manual chunk split, dev server on port 3000, `assetsInlineLimit: 0`

## Invariants
- Phaser is always emitted as a separate chunk (`manualChunks`) to avoid bundling it with app code [vite.config.ts]
- `assetsInlineLimit: 0` guarantees all assets are referenced as separate files, not inlined as base64 [vite.config.ts]

## Side Effects
- Filesystem writes to `dist/` on `npm run build` [vite.config.ts]
- Spawns a local HTTP server on `0.0.0.0:3000` on `npm run dev` [vite.config.ts]

## Verification
- Run: `npm run build` (exits 0, produces `dist/`)

## Subdirectories & Boundaries
- **[src](./src/README.md)**: Web prototype application bootstrap and Phaser game instance initialization.

