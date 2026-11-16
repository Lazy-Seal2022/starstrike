# Sprite Specification: Hangar Screen Showcase (`ui_screen_hangar`)

> **Category:** UI/UX Design System — Ship Selection & Loadout Screen  
> **Target Audience:** UI/UX Designer & Menu Concept Artist  
> **In-Game Role:** Pre-run ship selection, stat telemetry & tactical loadout  

---

## 1. Concept & Visual Identity

The **Hangar Screen** is the player's personal shipyard. It features a technical aerospace telemetry aesthetic where ships are presented on an illuminated rotating turntable pedestal under industrial overhead spotlights.

* **Screen Resolution:** **1920 x 1080 px** (Responsive scaling to 16:9 and 16:10).
* **Atmosphere:** Dark cavernous space station drydock with gantries, fuel conduit umbilicals, and holographic status screens.

---

## 2. Screen Layout & Wireframe

```
+-----------------------------------------------------------------------------+
| [HEADER: NAVAL DOCKS]                  [META CREDITS: # 4,500 GEMS] [ESC]   |
|-----------------------------------------------------------------------------|
| [LEFT: SHIP ROSTER]     |            [CENTER: TURNTABLE STAGE]              |
| > [DELTA SCOUT]         |                                                   |
|   [ARES FIGHTER]        |             [ ROTATING 3D / 2D SHIP ]             |
|   [GOLIATH MINER]       |             [ ORTHOGRAPHIC SHOWCASE ]             |
|   [VIPER INTERCEPTOR]   |                                                   |
|   [TITAN JUGGERNAUT]    |              (Holographic Floor Grid)             |
|   [VANGUARD CARRIER]    |---------------------------------------------------|
|   [SHADOW PHANTOM]      | [RIGHT: TELEMETRY]   | [TACTICAL LOADOUT]         |
|                         | SPEED:  [||||||  ]   | ACTIVE SKILL:              |
|                         | SHIELD: [||||||||]   | [ SHOCKWAVE ] [ BLINK ]    |
|                         | ENERGY: [|||||   ]   | [ EMP NOVA  ]              |
|                         | HARDPOINTS: 2 SLOTS  |                            |
|-------------------------+----------------------+----------------------------|
|                         |        [ LAUNCH MISSION ]                         |
+-----------------------------------------------------------------------------+
```

---

## 3. Required Visual Deliverables

1. `ui_hangar_background.png` (1920x1080 px) — Cavernous sci-fi hangar bay with drydock gantries.
2. `ui_hangar_pedestal_grid.png` (512x256 px) — Perspective circular floor turntable with glowing concentric grid rings.
3. `ui_hangar_roster_button.png` (280x56 px) — Chamfered ship selector tab (Normal, Hover, Selected, Locked states).
4. `ui_hangar_stat_gauge_bar.png` (200x16 px) — Segmented horizontal telemetry stat bar.
5. `ui_hangar_launch_button.png` (320x64 px) — High-impact golden/cyan deployment button with pulsing border.
