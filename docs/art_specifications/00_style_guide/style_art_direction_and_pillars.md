# Style Guide — Art Direction & Visual Pillars

> **Document Type:** Overarching Game Style Specification  
> **Audience:** Lead Artist, Art Directors, Concept Artists  
> **Target Aesthetic:** Neo-Retro High-Contrast Space Combat / Clean Industrial Cyberpunk  

---

## 1. Aesthetic Identity & World Context

**StarStrike** takes place in a brutal, industrial space-mining frontier contested by rogue automated defense armadas and pirate syndicates. The game's visual identity balances two distinct worlds:

```
  [ INDUSTRIAL FUNCTIONALISM ]                  [ HIGH-ENERGY CYBERPUNK ]
  Rugged titanium hulls, exposed fuel lines,     High-volt neon plasma, cyan deflector
  bolted composite armor, visible thrusters.     shields, chromatic warps, laser arcs.
```

The game rejects muddy photorealism and low-fidelity nostalgia in favor of **razor-sharp, bold vector-like graphic illustration** with intense post-processing bloom.

---

## 2. The Three Visual Pillars

### Pillar 1: High-Contrast Silhouette Readability (60+ FPS)
* In chaotic dogfights with 30+ simultaneous projectiles, drones, and floating asteroids, visual confusion is fatal.
* Every game object must be immediately identifiable solely by its outer silhouette:
  * **Player Ships:** Forward-pointing triangles, clean delta wings, balanced symmetry.
  * **Hostile Enemies:** Aggressive backward-swept mandibles, jagged claws, predatory raptor wings.
  * **Asteroids:** Organic faceted polygons with sharp crater bite notches.
  * **Projectiles:** Distinct geometric profiles (cylindrical laser bolts vs. round kinetic pellets vs. finned rockets).

### Pillar 2: Tactility & Mechanical Weight
* Ships are not toy spaceships; they are aerospace vessels with visible structural logic.
* Illustrators must show panel seam lines, rivets on heavy hulls, radiator heat sinks, and distinct hardpoint mounts where weapons physically bolt onto the chassis.
* Thruster bells and exhaust nozzles must look like they can realistically withstand rocket propulsion.

### Pillar 3: Visceral Visual Juice & Impact Feedback
* Every combat interaction must have an instant visual payoff:
  * Muzzle flashes illuminate the ship nose on every trigger pull.
  * Shields produce expanding hexagonal honeycomb ripples when absorbing laser fire.
  * Hull damage is shown through scorched armor, smoke plumes, and sparking circuits.
  * Fatal explosions shatter craft into physics-driven structural shrapnel chunks rather than merely vanishing.

---

## 3. Linework, Shading & Form Language

### Linework Standards
* **Outer Outline:** 2px solid dark contour (`#0B0F19`) outlining the ship silhouette to separate it cleanly from dark space backgrounds.
* **Inner Seam Lines:** 1px subtle dark slate lines (`#1E293B`) denoting armor panel joints and access hatches.
* **No Uncontrolled Gradients:** Use deliberate 2-to-3-tone cel-shading (Base Tone, Shadow Core Tone, Specular Highlight Edge) to preserve crisp graphic quality at high camera zoom levels.

### Form Hierarchy
```
    [ LIGHT SHIPS ]           [ MEDIUM SHIPS ]          [ HEAVY DREADNOUGHTS ]
   Thin, sharp wings         Angular swept plates       Chunky overlapping slabs
  Low surface profile       Visible side hardpoints     Multiple battery decks
```
