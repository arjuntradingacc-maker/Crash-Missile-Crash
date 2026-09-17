# Setup

## Requirements

- **Unity 2022.3 LTS** (the project targets `2022.3.50f1`; any 2022.3.x patch should work —
  Unity will offer to re-target `ProjectSettings/ProjectVersion.txt` on first open).
- Universal Render Pipeline, TextMeshPro, Input System, Addressables and Cinemachine packages are
  declared in `Packages/manifest.json` and will resolve automatically on first open (requires
  network access the first time). `com.unity.mobile.notifications` is deliberately **not**
  included — nothing in the project schedules local/push notifications yet, and that package
  adds its own Android manifest/runtime-permission requirements (`POST_NOTIFICATIONS` on API 33+,
  a notification icon resource) that would just be dead weight on the first Android build. Add it
  back if/when you implement re-engagement notifications.

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

## Mobile build settings

This is an Android-first project. The settings below apply to every platform; the dedicated
[Android build](#android-build-apk--aab) section covers the Android-specific steps in full.

- **Orientation**: `Project Settings > Player > Resolution and Presentation` → Portrait, lock
  Auto Rotation to Portrait only.
- **Color space**: Linear (URP default).
- **Target frame rate**: `GameManager.Awake()` already sets `Application.targetFrameRate = 60`
  and `QualitySettings.vSyncCount = 0`; this is the only place that needs to change if you want a
  different target.
- **Quality levels**: `SettingsScreen` exposes Low/Medium/High/Ultra buttons that call
  `QualitySettings.SetQualityLevel`; configure the actual per-level settings (shadow distance,
  texture quality, particle limits) in `Project Settings > Quality` to taste.
- **iOS** (secondary target, not the current focus): set a bundle identifier, enable IL2CPP, and
  fill in the usual `Info.plist` privacy strings if you wire up a real ads/IAP/analytics SDK
  later (none of the current mock implementations need any).

## Android build (APK / AAB)

Unity's Android build **is** a native Android app: `Build Settings > Android > Build` produces a
real `.apk` (or `.aab` for Play Store) containing compiled native code (IL2CPP → ARM machine
code) running through the standard Android `Activity`/`SurfaceView` lifecycle — there is no
wrapper, webview, or cross-platform shim involved at runtime.

### One-time platform setup

1. In Unity Hub, make sure the **Android Build Support** module (plus its **Android SDK & NDK
   Tools** and **OpenJDK** sub-components) is installed for your Unity 2022.3.x version. Unity
   Hub installs and manages the Android SDK/NDK/JDK for you — you do not need a separate Android
   Studio install unless you want one.
2. `File > Build Settings > Android > Switch Platform`.
3. `File > Build Settings > Player Settings > Player > Android tab`:
   - **Company Name / Product Name**: sets the default package id; override explicitly below.
   - **Other Settings > Identification**
     - **Package Name**: reverse-DNS id, e.g. `com.yourstudio.crashmissilecrash` (must match any
       `realMoneyProductId` prefixes you use later for real IAP products, and must be unique on
       the Play Store).
     - **Minimum API Level**: **Android 7.0 (API 24)** or higher (URP's minimum supported floor;
       24 gives broad device coverage for a 2026 release).
     - **Target API Level**: leave on **Automatic (highest installed)** so Play Store target-API
       requirements are met without manual bumps each year.
     - **Scripting Backend**: **IL2CPP** (required for 64-bit; Mono is not Play-Store-eligible
       for new apps).
     - **Target Architectures**: check **ARM64** (required by Play Store); include **ARMv7** too
       if you want to support older 32-bit-only devices, at the cost of a larger build.
   - **Other Settings > Configuration**
     - **Api Compatibility Level**: `.NET Standard 2.1` (default for 2022.3) is fine as-is.
   - **Publishing Settings**: this is where you create/select the upload **keystore** before a
     release build (`Create New Keystore` the first time, then keep that `.keystore` file and its
     passwords safe and out of source control — it is the only way to publish updates to the
     same app listing).
4. `Assets/Plugins/Android/AndroidManifest.xml` (already in this repo) declares the
   `INTERNET`/`ACCESS_NETWORK_STATE`/`VIBRATE` permissions the game needs (raids and the other
   backend-shaped calls go through `IBackendService`, even though the shipped implementation is
   a local offline mock; haptics use `VIBRATE`). It intentionally does not declare a custom
   `<application>`/`<activity>` block, so Unity's own generated manifest (whichever Activity
   backend your installed Editor version defaults to) is used as-is and this file is merged into
   it by Gradle — no version-specific Activity class name to get wrong.

### Building

- **Play Store submission**: `Build Settings > Build App Bundle (Google Play)` checked, then
  `Build`, producing an `.aab`. This is what you upload to Play Console.
- **Sideloading / device testing**: uncheck App Bundle, `Build And Run` with a device connected
  over USB (with USB debugging enabled) to get an installable `.apk` directly.
- **Icons**: `Player Settings > Icon` — until real art exists, Unity's default icon is used; swap
  in real adaptive icon layers there before shipping.

### Performance targets on Android

`GameManager` already locks `Application.targetFrameRate = 60` and disables vSync count so the
game targets 60 FPS rather than being capped by the default. Combined with the pooling/spatial-
grid/SRP-Batcher approach described in `docs/ARCHITECTURE.md`, this should hold on mid-range
2022+ Android hardware for the crowd sizes the sample levels use; profile with **Android Logcat**
(`Window > Analysis > Android Logcat` after installing the package) or the Unity Profiler over
USB once you have a device to test the actual placeholder-primitive/procedural-SFX build on.

## Running the offline vertical slice right now

With just the steps above (no content authoring, no real backend/ads/IAP needed) you can play
through: tutorial → World 1 levels 1-3 → World 1 boss, upgrade cards, open card packs in the
shop with starting currency, raid a procedurally-generated opponent, and claim mission/season
rewards — all fully offline against the local mock backend.
