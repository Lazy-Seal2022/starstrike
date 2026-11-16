# Style Guide — Color Palettes & Emissive Lighting

> **Document Type:** Overarching Game Style Specification  
> **Audience:** 2D Colorists, Texture Artists, Technical Artists  
> **Target Engine:** Unity 6 URP 2D Lighting Pipeline  

---

## 1. Functional Color Theory: Gameplay Binding

In StarStrike, color is strictly bound to gameplay function. Color informs the player of friend vs. foe, hazard proximity, resource value, and energy state without requiring text:

```
[ FUNCTION ]              [ PRIMARY TONE ]         [ HOT CORE / BLOOM ]     [ HEX RANGE ]
Player / Friendly         Electric Cyan            Pure White               #00F0FF / #FFFFFF
Hostile / Dangerous       Crimson / Blood Red      Hot Coral                #FF0055 / #FFE4E8
Shield Deflector          Cobalt / Royal Azure     Electric Light Blue      #3B82F6 / #DBEAFE
Capacitor Energy / Boost  High-Volt Yellow         Pale Lemon               #FACC15 / #FEF08A
Critical / Radiation      Molten Hazard Orange     White Gold               #F97316 / #FFEDD5
Mining Minerals / Gems    Radiant Emerald          Mint Foam                #10B981 / #A7F3D0
Tachyon / Subspace Warp   Ultra-Violet             Lavender White           #A855F7 / #F3E8FF
```

---

## 2. Standard Material Palettes (Chassis & Armor)

### Dark Chassis & Structure
* **Deep Void Undertone:** `#0B0F19` (Cavities, thruster interior walls, structural frame)
* **Gunmetal Base Plate:** `#1E293B` (Primary ship hull plating)
* **Shadow Crevice:** `#0F172A` (Panel gaps, underside shading)

### Metallic Hardware & Armor Accents
* **Titanium Midtone:** `#334155` (Armor bevels, wings, weapon pylons)
* **Ceramic Edge Highlight:** `#64748B` (Beveled edges catching directional light)
* **Naval White Trim:** `#F8FAFC` (High-contrast military stripes, hull identification markings)
* **Warning Hazard Yellow:** `#EAB308` (Used on industrial mining craft and explosive warheads)

---

## 3. Emissive Mask Architecture (URP 2D Bloom)

Unity Universal Render Pipeline (URP 2D) post-processing applies high-intensity bloom to all pixels with an emissive value greater than `1.0`.

To support this, every sprite with glowing components must provide an accompanying **Emissive Texture Mask**:

```
+---------------------------+       +---------------------------+
|      DIFFUSE TEXTURE      |  -->  |    EMISSIVE GLOW MASK     |
| [ship_player_fighter.png] |       | [ship_player_fighter_e.png]|
| Full color hull + cockpit |       | Black (0,0,0) everywhere  |
| and mechanical details.   |       | White/Color ONLY on glow! |
+---------------------------+       +---------------------------+
```

### Components Requiring Emissive Masks
1. **Cockpit Canopy Glass:** Cyan glow on player ships; ominous crimson glow on hostile drones.
2. **Engine Thruster Bells:** Pure white-hot centers with saturated flame halos.
3. **Weapon Muzzle Tips:** Faint standby luminescence.
4. **Wingtip Navigation Strobe Beacons:** 1px flashing safety dots.
5. **Alien / Crystal Mineral Veins:** Exposed fissures on asteroids and gems.
