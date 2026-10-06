# StarStrike: Unity

Unity version of StarStrike, side by side with the Phaser/web version in `../src`.
Targets: **Android, iOS, Windows, WebGL**, all from this one project.

## First-time setup

The Editor generates `Packages/` and `ProjectSettings/`, so they aren't committed yet:

1. Unity Hub → **New project** → Unity 6 LTS → **Universal 2D** template. Create it in any temp folder.
2. Move the new project's `Packages/` and `ProjectSettings/` into this `unity/` folder, then delete the temp project.
3. Unity Hub → **Add → Add project from disk** → select `unity/`.
4. Package Manager → install **Input System** (keyboard, mouse, touch and gamepad from one API).
5. Hub → Installs → add modules: **Android Build Support**, **WebGL Build Support**, **Windows Build Support (IL2CPP)**, and **iOS Build Support** (iOS builds need a Mac).
6. Run `git lfs install` once. Binaries are routed to LFS by [.gitattributes](.gitattributes).
7. Commit `Packages/`, `ProjectSettings/` and all `.meta` files.

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
