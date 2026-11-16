# Sprite Specification: Railgun Sniper (`enemy_sniper`)

> **Category:** Hostile Armada — Long-Range Artillery Specialist  
> **Target Audience:** 2D Creature/Vehicle Artist & Sprite Animator  
> **In-Game Role:** Standoff craft charging a piercing hypersonic beam  

---

## 1. Concept & Visual Identity

The **Railgun Sniper** is an elongated, asymmetric predator designed for extreme long-range standoff combat. Built around a massive magnetic accelerator cannon, it avoids close-quarters dogfights and kites away when approached.

* **Personality:** Cold, calculated, precise, lethal from afar.
* **Canvas Size:** **72 x 48 pixels** (2.25 x 1.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
  =========================================== [ EXTENDED RAILGUN BARREL ]
  (HYPERSONIC MUZZLE)   | [CAPACITOR 1] [CAPACITOR 2] [CAPACITOR 3] |
                        \=== [ STABILIZER COUNTERWEIGHT / ENGINES ] =/
```

### Exterior Details
* **Asymmetric Architecture:** Port side is dominated by the long railgun barrel; starboard side houses heavy capacitor heat sinks and stabilization fins.
* **Capacitor Coils:** 3 exposed electromagnetic induction rings glowing bright violet (`#A855F7`).
* **Hull Finish:** Dark slate armor plating (`#1E293B`) with matte black stealth shielding.

---

## 3. Attack Sequence & Visual Telegraphs

1. **Target Lock-On (1.2 Second Charge Time):**
   - The 3 capacitor coils illuminate sequentially from rear to front.
   - Emits a thin, pulsing purple laser targeting line (`LineRenderer`) connecting the barrel to the player ship.
   - Barrel tip sparks with swirling purple plasma motes.
2. **Hypersonic Release:**
   - Sudden blinding white-violet beam flash.
   - Distinct backwards mechanical recoil animation.
   - Dissipating heat smoke venting from the capacitor fins.

---

## 4. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Railgun Muzzle Tip** | `(X: +0.78, Y: +0.18)` | Origin for targeting line & sniper projectile. |
| **Cooling Vents** | `(X: -0.15, Y: -0.22)` | Anchor for heat distortion particle FX. |
| **Twin Micro-Thrusters** | `(X: -0.65, Y: -0.12)` | Maneuvering propulsion nozzles. |

---

## 5. Required Visual Deliverables

1. `enemy_sniper_diffuse.png` (72x48 px, 32-bit RGBA) — Neutral standby flight state.
2. `enemy_sniper_emissive.png` (72x48 px, 24-bit RGB) — Violet capacitor coils, targeting optic, and thruster cores.
3. `enemy_sniper_charge_sheet.png` (288x48 px spritesheet) — 4-frame capacitor charge progression (0%, 33%, 66%, 100% glow).
4. `enemy_sniper_recoil.png` (72x48 px, 32-bit RGBA) — 1-frame mechanical kickback frame.
