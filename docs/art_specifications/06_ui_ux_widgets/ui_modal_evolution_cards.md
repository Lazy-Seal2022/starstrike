# Sprite Specification: Evolution Selection Cards (`ui_modal_cards`)

> **Category:** UI/UX Design System — Mid-Run Progression Modal  
> **Target Audience:** UI/UX Designer & Card Illustrator  
> **In-Game Role:** Side-by-side branch choice cards during in-run ship evolution  

---

## 1. Concept & Visual Identity

When reaching level milestones during a run, combat pauses and presents the pilot with **two large holographic Evolution Cards** (e.g. *Ares Fighter* vs. *Goliath Miner*). These cards must look premium, tactile, and rewarding, showcasing high-resolution orthographic art of the hulls.

* **Card Dimensions:** **320 x 480 pixels** each (presented side-by-side).
* **Style:** Translucent dark obsidian glass backing (`#0B0F19` with 85% alpha) with chamfered neon tech borders.

---

## 2. Visual Anatomy of an Evolution Card

```
+----------------------------------------------------+
|  [ TIER BADGE ]             [ HULL CLASS HEADER ]  |
|  TIER 2 ASSAULT             ARES FIGHTER           |
|----------------------------------------------------|
|                                                    |
|                                                    |
|           [ HIGH-RESOLUTION SHIP ARTWORK ]         |
|             (Orthographic Angled Render)           |
|                                                    |
|                                                    |
|----------------------------------------------------|
|  PERKS & STAT MODIFIERS:                           |
|  [+] 2x Twin Forward Wing Cannons                  |
|  [+] +25% Turn Rate & Flight Top Speed             |
|  [-] -10% Shield Capacity                          |
|----------------------------------------------------|
|              [ SELECT THIS VESSEL ]                |
+----------------------------------------------------+
```

---

## 3. Interaction & Animation States

1. **Idle State:** Soft cyan border glow (`#00F0FF`, 40% intensity).
2. **Hover / Focus State:** Card scales up 5% (`1.05x`), border illuminates to 100% intensity, card backing shimmers with moving scanline lines.
3. **Selected Confirmation:** Card flashes brilliant white, plays an activation surge SFX, while the unselected card dissolves into particles.

---

## 4. Required Visual Deliverables

1. `ui_card_backing_default.png` (320x480 px, 32-bit RGBA) — Base glass card frame with chamfered brackets.
2. `ui_card_border_glow.png` (320x480 px) — Emissive bloom overlay for hover state.
3. `ui_card_button_select.png` (240x48 px) — Chamfered confirmation button with hover/pressed states.
4. `ui_card_scanlines_overlay.png` (320x480 px, transparent pattern) — Subtle animated holographic scanline texture.
