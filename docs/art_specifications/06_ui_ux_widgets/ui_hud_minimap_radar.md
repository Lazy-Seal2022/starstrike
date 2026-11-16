# Sprite Specification: HUD Minimap Radar (`ui_hud_radar`)

> **Category:** UI/UX Design System — Tactical Navigation  
> **Target Audience:** UI/UX Designer & Technical Artist  
> **In-Game Role:** 360-degree sweep radar displaying blips for entities  

---

## 1. Concept & Visual Identity

The **Minimap Radar** is a circular tactical scope positioned at the right-center of the screen. It provides situational awareness of nearby threats, large asteroids, and mined gems beyond the immediate camera viewport.

* **Canvas Size:** **160 x 160 pixels** (Scales to 180x180 px on 4K/1440p displays).
* **Style:** Transparent holographic glass backing with cyan range rings and a rotating radar sweep line.

---

## 2. Visual Blueprint & Scope Elements

```
            .-----[ 50m RANGE RING ]-----.
          /                                \
         |          /\ (PLAYER ARROW)       |
         |         /  \                     |   ===> (CONTINUOUS 360° SWEEP LINE)
         |            * (HOSTILE BLIP)      |
          \          . (ASTEROID DOT)      /
            '-----[ 100m RANGE RING ]----'
```

### Radar Entity Blips Legend
* **Player Ship:** High-contrast cyan triangle pointing in the ship's current flight heading.
* **Hostile Drone / Enemy:** Pulsing crimson diamond (`#FF0055`, 6x6 px).
* **Capital Flagship Boss:** Large flashing red skull glyph (`14x14 px`).
* **Asteroids:** Subtle slate-gray circular dots (`#64748B`, 4x4 px).
* **Gem Crystals:** Soft pulsing green diamond pings (`#10B981`, 4x4 px).

---

## 3. Required Visual Deliverables

1. `ui_radar_scope_frame.png` (160x160 px, 32-bit RGBA) — Scope border, chamfered corner brackets, cardinal compass tick marks (N, S, E, W).
2. `ui_radar_range_rings.png` (160x160 px) — Concentric distance rings (50m, 100m, 150m).
3. `ui_radar_sweep_line.png` (160x160 px, rotating gradient alpha) — 360-degree fading phosphor sweep line.
4. `ui_radar_blips_atlas.png` (64x64 px) — Sheet containing all 5 entity blip icons.
