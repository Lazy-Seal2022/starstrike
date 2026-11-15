# Audio Directory Structure

Single source of truth for all audio assets in StarStrike. Audio clips are organized by functional domain and referenced directly by `AudioConfig.asset` (`Assets/_Project/Resources/Data/AudioConfig.asset`).

## Subdirectories

- **`Music/`**: Background music and ambient sound loops.
  - `ambient_music.ogg`: Main space ambient background music.
- **`SFX/Combat/`**: Weapon fire, impact sounds, and explosions.
  - `laser.ogg`: Player primary laser fire.
  - `enemy_laser.ogg`: Hostile pulse / laser fire.
  - `explosion.ogg`: Ship destruction and impact detonation.
- **`SFX/World/`**: Environmental interactions and pickups.
  - `gem.ogg`: Mining / drop resource collection.
- **`SFX/UI/`**: Menu interactions and feedback sounds.
  - `click.ogg`: UI button click sound.
  - `upgrade.ogg`: Level up / perk upgrade notification chime.

## Architecture & Service Access

All runtime audio requests route through `AudioService`:
- `AudioService` is initialized by `GameBootstrapper` using `AudioConfig` loaded from `Resources/Data/AudioConfig.asset`.
- `AudioConfig` holds direct object references to the canonical clips in this directory.
- `ProceduralAudioHelper.cs` serves strictly as an in-memory procedural synthesizer fallback for unit testing and headless execution when audio files are absent.

## Import Settings & Optimization Guidelines

Per Unity audio best practices:
- **Music (`Music/`)**:
  - `Load Type`: Streaming (`AudioClipLoadType.Streaming`)
  - `Load in Background`: Enabled (`true`)
  - `Preload Audio Data`: Disabled (`false`)
  - `3D Sound`: Disabled (`2D`)
- **SFX (`SFX/Combat/`, `SFX/World/`, `SFX/UI/`)**:
  - `Load Type`: Decompress on Load (`AudioClipLoadType.DecompressOnLoad`) for zero playback latency
  - `Preload Audio Data`: Enabled (`true`)
  - `Force To Mono`: Enabled (`true`) for 50% memory reduction and correct spatial positioning
  - `UI Sounds`: `3D Sound` disabled (pure 2D stereo/mono bus)
