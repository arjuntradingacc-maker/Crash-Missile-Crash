# Architecture

## Guiding principles

1. **Content is data, not code.** Every unit, enemy, boss, cannon, champion, card, gate, level,
   world, mission, live event, season and shop offer is a JSON file under
   `Assets/Resources/Data/`. Adding content should never require touching a `.cs` file. See
   `Assets/Scripts/Data/` for the POCO models and `GameDatabase` for the loader/registry.
2. **Systems talk through events, not references.** `Assets/Scripts/Core/EventBus.cs` is a tiny
   generic pub/sub bus. Gameplay (Battle), meta-game (Cards/Cannons/Champions/Economy/...) and
   live-ops (Missions/Analytics/Audio) systems almost never hold a direct reference to each
   other — they publish/subscribe to small `readonly struct` events. This is why, for example,
   `AnalyticsManager` and `AudioManager` can both react to `BattleVictoryEvent` without either
   knowing the other exists, and why `MissionManager` can track "units multiplied" just by
   listening to `GateTriggeredEvent` instead of `BattleManager` having to know missions exist.
3. **Interfaces at every I/O boundary.** Anything that would talk to the outside world in
   production — a backend, an ad network, a store, an analytics SDK, remote config — is an
   interface (`IBackendService`, `IAdProvider`, `IIAPProvider`, `IAnalyticsProvider`,
   `IRemoteConfigProvider`) with a local/offline implementation registered by default. Swapping
   to a real provider means writing one new class and changing one `new XyzProvider()` line.
4. **`ServiceLocator` for cross-cutting lookups, singletons for in-module access.** Each manager
   exposes a static `Instance` for the systems that always need it (e.g. `CrowdManager.Instance`
   from inside `Battle/*`), and additionally registers itself with `ServiceLocator` so unrelated
   modules can do a soft, null-safe `ServiceLocator.TryGet<T>(out var manager)` without a hard
   compile-time dependency direction.
5. **No hand-authored binary Unity assets.** There is no `.unity` scene beyond the one empty
   bootstrap object the user creates (see SETUP.md), no `.prefab` files, and no ScriptableObject
   `.asset` files. `GameBootstrap` builds every manager and the entire UI at runtime in code;
   `VisualRegistry` + `PrimitiveFactory` generate placeholder visuals procedurally; UI is built
   by `UIBuilder`/`ScrollListFactory`. This was a deliberate response to not having a Unity
   Editor available to validate hand-authored binary/YAML assets — everything that exists is
   plain, readable, statically-checkable C# and JSON.

## Layers

```
Data/            Plain C# POCOs + GameDatabase (loads Resources/Data/*.json once at boot)
Core/            EventBus, ServiceLocator, ObjectPool<T>, GameManager (app state machine),
                 GameBootstrap (builds everything), GameFlowController (battle <-> meta glue),
                 VisualRegistry (data key -> placeholder/real visual)
Battle/          The moment-to-moment game: CannonController/CannonFireController, CrowdManager,
                 EnemyManager, CombatSystem (spatial-grid targeting), Gates/, SpecialMechanics/,
                 EnemyBase, BossController, CameraController, VFXManager, BattleManager
Champions/       ChampionUnit (fights alongside the crowd) + ability execution + ChampionManager
Cannons/         CannonManager (unlock/equip; upgrade levels delegate to Cards)
Cards/           CardManager (collection + upgrade), CardPackManager (weighted pack rolls)
Economy/         EconomyManager (currencies), InventoryManager (cosmetics)
Progression/     ProgressionManager (XP), MissionManager, LeagueManager, SeasonManager
Base/            BaseManager (building levels, shield)
Raids/           RaidManager, RevengeManager, DefenseLayout (talks to IBackendService)
Events/          EventManager (live/limited-time events, format-agnostic)
Backend/         IBackendService + LocalMockBackendService, SaveManager, PlayerSaveData
Ads/ IAP/        IAdProvider/IIAPProvider + mock implementations + Manager wrappers
Analytics/       IAnalyticsProvider + ConsoleAnalyticsProvider + AnalyticsManager (event-driven)
RemoteConfig/    IRemoteConfigProvider + LocalRemoteConfigProvider + RemoteConfigManager
UI/              UIBuilder/ScrollListFactory (procedural uGUI), UIManager (screen stack),
                 UI/Screens/* (one class per screen)
Audio/           AudioManager (event-driven SFX via ProceduralAudioFactory, music hooks)
Tutorial/        TutorialManager (scripted 11-step onboarding coroutine)
Utils/           PrimitiveFactory (placeholder visuals), ProceduralAudioFactory (placeholder SFX)
```

## The battle loop, concretely

`BattleManager.StartLevel(LevelData)` is the single entry point for a battle attempt. Each
frame it ticks, in this fixed order, every subsystem that needs to run:

1. `GateManager.Tick` — moves gates (sliding/rotating/timed) and checks whether any active
   friendly unit has entered an un-triggered gate's trigger volume; if so, applies the gate's
   math to the crowd's *current total count* once (not per-unit — see the doc comment on
   `GateController.TryConsume` for why per-unit application doesn't match the spec's worked
   examples).
2. `ObstacleManager.Tick` — speed zones, launch pads, moving platforms, crushers, teleporters,
   moving walls; all implemented as cheap AABB-overlap checks against active units rather than
   Unity physics colliders, per the "avoid expensive per-unit physics" performance guidance.
3. `CombatSystem.Tick` — rebuilds two `SpatialGrid<ICombatTarget>` instances (friendly, enemy)
   from every currently-alive combatant (crowd units, the champion, enemy units, base towers,
   the base core, destructible barriers) and resolves targeting/attacks. Units keep a locked
   target until it dies rather than re-querying every frame.
4. `CrowdManager.Tick` / `EnemyManager.Tick` / `ChampionManager.Tick` — movement, using the
   target locked by step 3 to decide "advance" vs. "hold and fight".
5. Boss tick, timers, and win/lose condition checks.

`ICombatTarget` is the interface that lets friendly units, enemy units, base towers, the base
core, and destructible barriers all be targeted/damaged uniformly — see `Battle/ICombatTarget.cs`.

## Performance approach

- **Object pooling** (`Core/ObjectPool.cs`) for every unit type, keyed per content id, so
  hundreds/thousands of units never trigger per-unit `Instantiate`/`Destroy`.
  - **Movement/combat are driven by plain method calls from one `Update`**, not per-object
  `MonoBehaviour.Update()` dispatch — `CrowdManager`/`EnemyManager` iterate their active-unit
  lists once per frame and call a plain C# method, which avoids Unity's native↔managed message
  dispatch overhead per GameObject.
- **Spatial partitioning** (`Battle/SpatialGrid.cs`) turns "nearest enemy in range" from an
  O(n²) crowd-vs-crowd problem into a bucketed lookup, rebuilt cheaply once per tick.
- **SRP Batcher-friendly rendering**: pooled units share one `Material` per content id (created
  once by `PrimitiveFactory`), which URP's SRP Batcher can batch efficiently even without
  explicit GPU instancing. `docs/ARCHITECTURE.md`'s "Known limitations" in the README covers the
  further step (`Graphics.DrawMeshInstancedIndirect`) for crowds beyond a few thousand.
- **JSON content loads once**, synchronously, from `Resources` at boot (`GameDatabase.LoadAll`),
  so normal campaign play never depends on the network.
