# scripts

## Overview
Project synchronization and cross-environment deployment utilities.

## Interface
- `scripts/sync-unity.sh` CLI executable

## Invariants
- POSIX bash shell compatibility [sync-unity.sh]

## Side Effects
- Filesystem directory sync between WSL and Windows paths [sync-unity.sh]

## Verification
- Run: `bash scripts/sync-unity.sh status`

