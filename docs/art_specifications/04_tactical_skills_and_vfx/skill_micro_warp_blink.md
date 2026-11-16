# Sprite Specification: Micro-Warp Blink (`skill_blink`)

> **Category:** Tactical Skills — Active Skill 2 Subspace Teleport  
> **Target Audience:** 2D FX Artist & Post-Processing Specialist  
> **In-Game Role:** Instant forward warp with invulnerability and afterimage trails  

---

## 1. Concept & Visual Identity

The **Micro-Warp Blink** allows the player to vanish from physical space and instantaneously reappear 8.5 units forward along their nose heading. It communicates high-tech electronic evasion and brief dimensional phase shifting.

* **Color Palette:** Electric Cyan (`#00F0FF`) with Subspace Violet fringes (`#A855F7`).

---

## 2. Visual Sequence Breakdown

```
[ 1. DEMATERIALIZE ]            [ 2. AFTERIMAGE GHOST TRAIL ]         [ 3. ARRIVAL SNAP ]
Ship collapses into a           3 semi-transparent silhouette ghosts  Sudden chromatic snap
horizontal streak of light.     fade from 60% to 0% alpha over path.  with radial vacuum pull.
```

1. **Origin Dematerialization:** A blinding horizontal light streak and flash sprite at the departure coordinate.
2. **Ghost Trail:** Spawns 3 fading translucent silhouette sprites of the player's active ship along the warp line.
3. **Arrival Compression:** Sharp chromatic aberration pulse at the destination coordinate as the ship snaps back into physical space.

---

## 3. Required Visual Deliverables

1. `vfx_warp_flash_origin.png` (48x48 px, 3-frame flipbook) — Imploding dimensional light flare.
2. `vfx_warp_arrival_burst.png` (64x64 px, 3-frame flipbook) — Expanding chromatic arrival flare.
3. `icon_skill_blink.png` (64x64 px, 32-bit RGBA) — Warp streak chevron vector icon for HUD.
