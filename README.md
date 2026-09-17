# Crash Missile Crash

An **Android** vertical mobile game in the crowd-multiplication / lane-control / base-destruction
genre (the "Mob Control-like" genre), built from scratch with an original identity: **Crash
Missile Crash**. You steer a rolling missile-cannon down a battlefield, firing an ever-growing
crowd of troops through multiplier gates, past hazards and enemy guards, into an enemy
stronghold you have to level.

Built in Unity/C#, which compiles to a genuine native Android app (IL2CPP → ARM machine code
inside a standard Android `Activity`, packaged as a real `.apk`/`.aab`) — this is not a
webview/hybrid wrapper. See [docs/SETUP.md](docs/SETUP.md#android-build-apk--aab) for the full
Android build walkthrough.

No characters, art, audio, UI, code, or level content from any existing commercial game is
used anywhere in this project. All game design documents, data, and creative direction in this
repository are original.

## Status

This repository contains a **complete, playable vertical slice plus the full architectural
skeleton for a live-service production game**, all in Unity C#, built and iterated on entirely
through this coding session. It was **not compiled or run inside the Unity Editor** during
development (no Editor is available in this environment) — see [Known limitations](#known-limitations-read-this-first)
before treating anything here as ship-ready.

What *is* here, and works together as one coherent system:

- A fully playable core loop: drag-to-aim cannon, continuous firing, pooled crowd of hundreds of
  units, data-driven multiplier/math gates with several motion types, automatic crowd-vs-crowd
  combat, an enemy base (towers + core) to destroy, boss fights with phases, victory/defeat flow,
  and an 11-step scripted tutorial.
- A complete meta-game: cards/collection, cannon and champion unlock+upgrade, currencies, XP,
  missions, leagues, a season pass, base building with shields, and asynchronous raids/revenge.
- Live-ops plumbing: backend, ads, IAP, analytics and remote-config are all defined as
  **interfaces** with working **local/offline implementations**, so every online feature is
  exercised end-to-end without a server, and swapping in a real backend/ad network/store touches
  exactly one class each.
- A UI covering every screen in the spec (home, battle HUD, victory/defeat, collection, shop,
  missions, season, events, base, world map, settings), built **procedurally in code** rather
  than as hand-authored Unity prefabs/scenes.
- Ten themed worlds' worth of data-driven content scaffolding (grassland through futuristic
  megacity), with a handful of fully authored sample levels demonstrating the pattern, and a
  first boss encounter.

## Quick start

See **[docs/SETUP.md](docs/SETUP.md)** for the one manual step needed to open this in Unity
(there is no hand-authored `.unity` scene file — the whole game is built by one bootstrap
script) and how to run it on device.

## Documentation

- **[docs/SETUP.md](docs/SETUP.md)** — opening the project, the one manual scene-setup step,
  build settings for iOS/Android.
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** — how the systems fit together, the
  event-driven decoupling pattern, why the game has almost no hand-authored binary assets.
- **[docs/EXTENDING.md](docs/EXTENDING.md)** — step-by-step recipes for adding new Units,
  Enemies, Cannons, Champions, Gates, Levels, Worlds, Events, Seasons, Missions and Shop
  products **without touching gameplay code**.

## Known limitations (read this first)

This was produced in a single automated coding session with **no Unity Editor, no compiler, and
no device available to test on**. Being upfront about what that means:

- **Unverified compile/runtime correctness.** Every script was written and manually
  cross-checked for C# and Unity API correctness, and a dedicated self-review pass went through
  the whole codebase hunting for namespace/reference bugs (several were found and fixed). But it
  has never actually been compiled by Unity or pressed play on. Treat first-open-in-Editor as
  part of the job, not a formality — budget time for a debugging pass.
- **No art, music or sound design.** Every visual is a procedurally-generated colored primitive
  (`Utils/PrimitiveFactory.cs`), and every sound effect is a procedurally-synthesized tone
  (`Utils/ProceduralAudioFactory.cs`). This keeps the game fully playable and testable with zero
  asset dependencies, but it is a placeholder toy-block look, not final art. Swapping in real
  models/animations/audio is a drop-in replacement — see `VisualRegistry` and `AudioManager`.
- **Ten worlds are scaffolded, not fully populated.** `worlds.json` defines all ten themed worlds
  from the spec end to end (naming, order, unlock chain), and the level/gate/obstacle/enemy data
  format supports arbitrarily many levels with zero code changes, but only a handful of sample
  levels (tutorial + World 1, levels 1-3 + a boss) are actually authored as content. Filling out
  the other ~115 levels is pure JSON authoring — see EXTENDING.md.
- **Backend, ads, IAP are local/offline mocks.** They exist as real interfaces
  (`IBackendService`, `IAdProvider`, `IIAPProvider`) with fully working offline implementations
  so the whole raid/rewarded-ad/purchase flow is testable, but nothing is actually live. Going
  to production means implementing those three interfaces against a real backend, ad SDK, and
  Unity IAP/store — the rest of the game does not change.
- **No automated tests.** Given the environment, none were written. The architecture (small,
  single-purpose classes, interfaces at every I/O boundary) is deliberately test-friendly; adding
  an EditMode/PlayMode test suite is a natural next step.

None of this is hidden inside the code — every mock/placeholder says so in its own doc comment.

## License

See [LICENSE](LICENSE).
