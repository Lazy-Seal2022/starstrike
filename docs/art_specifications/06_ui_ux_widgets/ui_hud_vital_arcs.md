# Sprite Specification: HUD Vital Status Arcs (`ui_hud_vitals`)

> **Category:** UI/UX Design System — Pilot Flight Telemetry  
> **Target Audience:** UI/UX Designer & Motion Graphic Artist  
> **In-Game Role:** Curved concentric meters displaying Shield, Energy & Hull  

---

## 1. Concept & Visual Identity

The **Vital Status Widget** sits at the top-left of the pilot's display. Inspired by fighter jet holographic helmet displays, it uses **curved concentric circular arcs** rather than flat rectangular progress bars to convey ship health and capacitor energy.

* **Style:** High-contrast cyan/blue vector lines, 45-degree chamfers, digital tick marks.
* **Anchor:** Top-Left screen quadrant (`Anchor: UpperLeft`).

---

## 2. Visual Anatomy & Blueprint

```
       .-----------------------.
     /   [ OUTER SHIELD ARC ]    \    (Electric Blue #3B82F6 - Radial Fill)
    |  /   [ INNER ENERGY ARC ]   \ | (Cyan/Yellow #00F0FF - Radial Fill)
    | |                           | |
    |  \                         /  |
     \   [ SEGMENTED HULL BAR ]  /    (Green/Yellow/Red Segmented Health)
       '-----------------------'
```

1. **Outer Shield Arc (180° Curved Gauge):**
   - Electric Blue (`#3B82F6`).
   - Radial fill with clock-wise depletion. Flashes white upon absorbing hits; strobes red when depleted.
2. **Inner Energy Arc (150° Concentric Arc):**
   - High-Volt Cyan (`#00F0FF`).
   - Depletes dynamically when boosting or casting active skills; recharges smoothly with an illuminated energy tip.
3. **Hull Integrity Bar:**
   - 10-segment horizontal tick bar situated directly beneath the circular arcs.
   - Transitions from Green (`#10B981`) -> Warning Yellow (`#EAB308`) -> Critical Hazard Red (`#EF4444`).

---

## 3. Required Visual Deliverables

1. `ui_hud_vitals_frame.png` (256x256 px, 32-bit RGBA) — Static background gauge frame with chamfered brackets and tick marks.
2. `ui_hud_shield_arc_fill.png` (256x256 px) — Clean circular radial fill sprite mask for Shield.
3. `ui_hud_energy_arc_fill.png` (256x256 px) — Clean concentric circular fill sprite mask for Energy.
4. `ui_hud_hull_segment.png` (16x8 px) — Single segmented hull tick box.
