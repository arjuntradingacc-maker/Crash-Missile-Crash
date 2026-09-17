# Setup

## Requirements

- **Unity 2022.3 LTS** (the project targets `2022.3.50f1`; any 2022.3.x patch should work —
  Unity will offer to re-target `ProjectSettings/ProjectVersion.txt` on first open).
- Universal Render Pipeline, TextMeshPro, Input System, Addressables, Cinemachine and Mobile
  Notifications packages are declared in `Packages/manifest.json` and will resolve automatically
  on first open (requires network access the first time).

## Opening the project

This repository deliberately ships **no hand-authored `.unity` scene file**. Every manager,
the battlefield, the camera, and the entire UI are built procedurally at runtime by
`Assets/Scripts/Core/GameBootstrap.cs`. This was a conscious choice: `.unity`/`.prefab` files
are binary/YAML assets that Unity itself must generate and validate, and there was no Unity
Editor available while writing this project to verify hand-authored versions of them would
actually open cleanly. A procedural bootstrap is plain, readable C# that can be verified by
inspection instead.

To get a playable scene:

1. Open the project folder in Unity Hub (`Add project from disk`), let it resolve packages.
2. In the Editor, create a new empty scene: `File > New Scene`, choose the **Basic (Built-in)**
   or **Empty** template, then immediately delete the default `Main Camera` and `Directional
   Light` GameObjects if the template included them (`GameBootstrap` creates its own).
3. Create one empty GameObject (`GameObject > Create Empty`), name it `GameBootstrap`, and add
   the `GameBootstrap` component to it (`Add Component > Crash Missile Crash > Core >
   GameBootstrap`, or just search "GameBootstrap").
4. Save the scene (e.g. as `Assets/Scenes/Main.unity`) and add it to
   `File > Build Settings > Scenes In Build` as index 0.
5. Press Play. `GameBootstrap.Awake()` builds every manager, the battlefield, camera and full UI,
   then shows the Home screen (or launches the tutorial on first run).

That's the entire manual setup. Everything else — content, levels, UI — is data or code that
loads automatically.

## Render pipeline

The `manifest.json` pulls in `com.unity.render-pipelines.universal`. After first import, create
a URP asset (`Assets > Create > Rendering > URP Asset (with Universal Renderer)`) and assign it
under `Project Settings > Graphics > Scriptable Render Pipeline Settings` if Unity doesn't do
this automatically for a URP-templated project. `PrimitiveFactory` (the placeholder-visual
system) looks for the `Universal Render Pipeline/Lit` shader and falls back to `Standard` if URP
isn't active yet, so the game is playable either way while you're setting this up.

## Input

Gameplay input (`CannonController`) uses the legacy `UnityEngine.Input` API rather than the new
Input System package, specifically so the project works immediately with **Active Input
Handling** set to either `Input Manager (Old)` or `Both` in `Project Settings > Player` — pick
`Both` if you plan to also wire up the Input System package for editor testing with a mouse. Do
**not** set it to `Input System Package (New)` only, or `CannonController` will stop receiving
input.

## Mobile build settings (iOS/Android)

- **Orientation**: `Project Settings > Player > Resolution and Presentation` → Portrait, lock
  Auto Rotation to Portrait only.
- **Color space**: Linear (URP default).
- **Target frame rate**: `GameManager.Awake()` already sets `Application.targetFrameRate = 60`
  and `QualitySettings.vSyncCount = 0`; this is the only place that needs to change if you want a
  different target.
- **Quality levels**: `SettingsScreen` exposes Low/Medium/High/Ultra buttons that call
  `QualitySettings.SetQualityLevel`; configure the actual per-level settings (shadow distance,
  texture quality, particle limits) in `Project Settings > Quality` to taste.
- **Android**: minimum API level 24+ recommended for URP; enable IL2CPP + ARM64.
- **iOS**: set a bundle identifier, enable IL2CPP, and (for TestFlight/App Store) fill in the
  usual `Info.plist` privacy strings if you wire up a real ads/IAP/analytics SDK later (none of
  the current mock implementations need any).

## Running the offline vertical slice right now

With just the steps above (no content authoring, no real backend/ads/IAP needed) you can play
through: tutorial → World 1 levels 1-3 → World 1 boss, upgrade cards, open card packs in the
shop with starting currency, raid a procedurally-generated opponent, and claim mission/season
rewards — all fully offline against the local mock backend.
