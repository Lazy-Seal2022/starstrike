# StarStrike: Unity

Unity version of StarStrike, side by side with the Phaser/web version in `../src`.
Targets: **Android, iOS, Windows, WebGL**, all from this one project.

## WSL & Windows Development Workflow (Option 2)

The Git repository lives in WSL (`/home/quan/projects/starstrike`), while the live Unity 6 Editor runs in Windows (`C:\Data\Unity\StarStrike`).

To synchronize between WSL and Windows:
- **Pull changes from Windows to Git**:
  ```bash
  npm run sync:unity:pull
  ```
  *(Mirrors updated assets, scenes, and settings from Windows into `unity/`, ready to commit)*

- **Push changes from Git to Windows**:
  ```bash
  npm run sync:unity:push
  ```
  *(Mirrors repo changes directly into the live Windows Unity project)*

Both projects share the identical folder structure and assembly definitions.


## Layout

```
unity/
├── Assets/
│   ├── _Project/                 # Everything we own (the underscore keeps it at the top)
│   │   ├── Art/                  # Sprites, Materials, VFX, Animations
│   │   ├── Audio/                # Music, SFX
│   │   ├── Data/                 # ScriptableObjects: ship definitions, game tuning (= web src/config)
│   │   ├── Prefabs/              # Ships, Enemies, Environment, Projectiles
│   │   ├── Scenes/               # Boot, Game
│   │   ├── Settings/             # Input actions, URP/quality per platform, Build Profiles
│   │   ├── UI/                   # Fonts, UI layouts
│   │   └── Scripts/
│   │       ├── Core/             # StarStrike.Core: pure C#, NO UnityEngine (ECS, events, types)
│   │       ├── Platform/         # StarStrike.Platform: save, haptics, ads/IAP; the only place for #if UNITY_ANDROID/IOS
│   │       ├── Gameplay/         # StarStrike.Gameplay: one folder per feature (AI, Combat, Mining, Movement, ...)
│   │       ├── UI/               # StarStrike.UI: HUD, Minimap
│   │       └── Editor/           # StarStrike.Editor: editor-only tools and per-platform build scripts
│   ├── Plugins/Android|iOS/      # Native plugins and manifest overrides (Unity special folders)
│   └── ThirdParty/               # Asset Store / external packages, never edited in place
├── Packages/                     # (generated) package manifest
└── ProjectSettings/              # (generated)
```

## Assembly dependency rule

```
Core  ←  Platform  ←  Gameplay  ←  UI        Editor → all (editor only)
```

- `Core` has `noEngineReferences: true`, so it compiles without Unity. Logic there is portable and unit-testable.
- Platform-specific code (`#if UNITY_ANDROID`, `UNITY_IOS`, `UNITY_WEBGL`, `UNITY_STANDALONE_WIN`) goes **only** in `Platform/`. Gameplay never branches on platform.
- Input goes through the Input System action asset in `Settings/Input`. Gameplay reads actions, not devices, so touch, keyboard and gamepad work automatically.
- New feature = new folder in `Scripts/Gameplay/`. New ship = new ScriptableObject in `Data/Ships/`.
