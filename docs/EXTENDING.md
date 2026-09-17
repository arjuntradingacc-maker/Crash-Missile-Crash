# Extending the game

Everything below is **pure content authoring** — add JSON, no code changes, no rebuild of
gameplay logic. All files live under `Assets/Resources/Data/`. Every list-shaped file is a JSON
object with a single `"items"` array (a JsonUtility quirk — it can't deserialize a bare array at
the document root, see `Data/DataCollections.cs`).

## Add a new friendly Unit

1. Add an entry to `units.json` with a unique `id`, stats, `rarity` (0=Common..4=Mythic), and a
   `visualKey` (any string — if no real prefab is registered for it in `VisualRegistry`, a
   colored placeholder capsule is generated automatically).
2. Add a matching card to `cards.json`: `targetType: 0` (Unit), `targetId` = the unit's id, plus
   upgrade cost curves.
3. Reference the unit's id from a `CannonData.spawnUnitId` (see below) so a cannon actually fires
   it, and/or from a level's `startingUnits` context.

## Add a new Enemy

Add an entry to `enemies.json` (`behavior`: 0=Melee, 1=Ranged, 2=Tank, 3=Fast, 4=Defensive,
5=Support, 6=Special). Reference its `id` from a level's `enemyWaves[].enemyId`. Behavior is
currently uniform (aggro range + move-to-target + attack); to give a behavior type unique logic
(e.g. Ranged enemies standing off at range, Support healing allies), extend `EnemyUnit.cs` with a
`switch` on a `behavior` field read from `EnemyData` — the data model already carries it.

## Add a new Boss

Add an entry to `bosses.json` with `phases` (each triggers when boss health drops to its
`healthThreshold01`, applying `damageMultiplier`/`moveSpeedMultiplier` and swapping in an
`abilityId`), a `minionEnemyId` for periodic adds, and enrage timing. Set a level's
`isBossLevel: true` and `bossId` to reference it. `BossController` already handles phase
transitions, minion waves and enrage — `abilityId` is currently just published as an event
(`BossAbilityUsedEvent`) for VFX/animation hookup; give it real mechanical effect by handling
that event (e.g. in a new `BossAbilityExecutor`, mirroring `Champions/ChampionAbilityExecutor.cs`).

## Add a new Cannon

Add an entry to `cannons.json` (`projectileType`: 0=SingleUnit, 1=SplitShot, 2=PiercingShot,
3=HomingShot — currently `unitsPerShot`/`damageMultiplier` drive the gameplay difference; the
enum is there for future per-type firing patterns) and a matching card in `cards.json`
(`targetType: 1`). It becomes selectable the moment `CannonManager.Unlock(id)` is called (the
Collection screen's Cannons tab calls this automatically the first time a player upgrades that
cannon's card past level 0).

## Add a new Champion

Add an entry to `champions.json`. `archetype` is cosmetic/organizational (Tank, Berserker,
Commander, Ninja, Engineer, Mage); `ultimateType` picks the mechanical effect from
`UltimateAbilityType` (AreaSlam, LightningStrike, ClonePlatoon, FrostNova, RallyBoost, Titan) —
see `Champions/ChampionAbilityExecutor.cs` for exactly what each does with `ultimatePower`/
`ultimateRadius`. Add a matching card (`targetType: 2`). To add a genuinely new ultimate
*behavior* (not just reusing one of the six), add a new `UltimateAbilityType` enum value and a
`case` in `ChampionAbilityExecutor.Execute`.

## Add a new Gate

Gates are two-part: a reusable **definition** (`gates.json` — the math + motion behavior) and a
per-level **placement** (a level's `gates[]` array — where it sits). `operation`: 0=Multiply,
1=Add, 2=Subtract, 3=Divide, 4=SetValue, 5=RandomRange (uses `randomMin`/`randomMax`).
`motion`: 0=Stationary, 1=Sliding (oscillates within `motionRange` at `motionSpeed`), 2=Rotating
(cosmetic spin), 3=Timed (alternates open `openDuration`/closed `closedDuration` — a closed gate
simply has no effect on units that reach it, they pass through untouched). Set `splitGroupId` on
two co-located placements to mark them as alternative paths (purely a level-design/documentation
hint currently — both still resolve independently via `GateController.TryConsume`).

## Add a new special level mechanic (obstacle)

Add an entry to a level's `obstacles[]` with a `type` from `ObstacleType` (SpeedZone, LaunchPad,
MovingPlatform, RotatingObstacle, Crusher, EnemyCannon, DefensiveTower, Bridge, Teleporter,
DestructibleBarrier, MovingWall) and the generic `paramA/B/C`/`health`/`linkedTeleporterId`
fields (their meaning is documented per-type in `Data/ObstacleData.cs` and implemented in
`Battle/SpecialMechanics/ObstacleBehaviours.cs`). To add a genuinely new mechanic type, add an
enum value, a new `ObstacleBehaviour` subclass, and a `case` in `ObstacleManager.BuildLevel`.

## Add a new Level

Create a new file under `Assets/Resources/Data/Levels/<id>.json` following the shape of the
existing samples (`tutorial_level.json`, `world1_level1.json`, ...), and add its resource path
(`"Data/Levels/<id>"`) to `level_index.json`'s `levelResourcePaths` array, in the order you want
it to appear in the campaign (`GameDatabase.GetNextLevel` walks this list in order). `difficulty`
coarsely scales enemy/boss/tower stats (`1 + (difficulty-1) * 0.12`, see `BattleManager` and
`LevelManager`); `timeLimitSeconds` is the fail-safe defeat timer if the base isn't destroyed in
time.

## Add a new World

Add an entry to `worlds.json` (`environmentKey` picks a ground/theming color in
`LevelManager.EnvironmentColor` — add a new `case` there for a genuinely new palette, or swap in
real environment art via that same method) and give its levels a matching `worldId`. `worlds.json`
already defines all ten spec'd worlds (Grassland → Futuristic Megacity) with the correct unlock
chain; you mostly just need to author more `worldId`-tagged levels.

## Add a Mission / Live Event / Season tier / Shop offer

- **Mission**: add to `missions.json`. `metric` must be one of `MissionMetric` — if you need a
  new countable thing, add an enum value and publish the corresponding progress from wherever it
  happens (`MissionManager` already listens broadly; add one more `EventBus.Subscribe`).
- **Live Event**: add to `events.json`. `format` (`LiveEventFormat`) is currently organizational;
  give a format unique rules by branching on it inside `EventManager` or a new per-format
  controller — the framework (time window, score, reward tiers, optional leaderboard) is generic.
- **Season tier**: add to the `tiers` array of the season in `seasons.json`.
- **Shop offer**: add to `shop.json`. Set `realMoneyProductId` for a real-money product (routes
  through `IAPManager`/`IBackendService.ValidatePurchaseAsync`), or leave it blank and set
  `coinCost`/`gemCost` for a soft-currency offer.

## Swapping placeholder art/audio for the real thing

- **Visuals**: `VisualRegistry` (a component on the `VisualRegistry` GameObject `GameBootstrap`
  creates) has an `overrides` list in the inspector — map any `visualKey` string used in the JSON
  content to a real prefab there, and every system that currently gets a generated placeholder
  for that key will use the real prefab instead, with zero code changes.
- **Audio**: `AudioManager` has public `AudioClip` fields for music layers; assign real composed
  tracks there. SFX are currently always the procedural tones in `ProceduralAudioFactory` — to
  use real sound-designed SFX, change `AudioManager.Play(...)` to look up an assigned `AudioClip`
  for the given key first and only fall back to the procedural tone if none is assigned.

## Swapping the backend/ads/IAP/analytics/remote-config providers

Each has exactly one line to change:

| System | File | Line to change |
|---|---|---|
| Backend | `GameBootstrap.BuildCoreSystems` | `new LocalMockBackendService()` → your `IBackendService` |
| Ads | `Ads/AdManager.Awake` | swap `MockAdProvider` for a real `IAdProvider` |
| IAP | `IAP/IAPManager.Awake` | swap `MockIAPProvider` for a real `IIAPProvider` |
| Analytics | `Analytics/AnalyticsManager.Awake` | swap `ConsoleAnalyticsProvider` for a real `IAnalyticsProvider` |
| Remote Config | `RemoteConfig/RemoteConfigManager.Awake` | swap `LocalRemoteConfigProvider` for a real `IRemoteConfigProvider` |

No other file needs to know a real network/store/SDK exists.
