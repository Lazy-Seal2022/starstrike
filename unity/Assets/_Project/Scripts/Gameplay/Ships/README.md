# Ships

## Overview
Vessel movement physics, inertial dampening, carrier deck operations, and escort drone management.

## Components
- `ShipMovement.cs`: Newtonian flight physics, rotation interpolation, thrust impulse, and speed limits.
- **Carrier/**:
  - `CarrierHangarController.cs`: Drone launch bays, stance commands (Defensive, Strike, Repair), and bay capacity.
  - `CarrierEscortDrone.cs`: Companion drone behavior, escort orbit formation, and autonomous target engagement.
