using CrashMissileCrash.Cannons;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Orchestrates a single battle attempt end-to-end: loads the level, ticks every battle
    /// subsystem in a fixed order each frame, and resolves victory/defeat. This is the only
    /// class that needs to know the full list of battle subsystems - everything else only knows
    /// about the pieces it directly touches.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private LevelData _level;
        private float _timeRemaining;
        private float _noUnitsTimer;
        private const float NoUnitsGraceSeconds = 5f;
        private bool _battleActive;
        private BossController _currentBoss;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void OnEnable() => EventBus.Subscribe<BaseCoreDestroyedEvent>(OnCoreDestroyed);
        private void OnDisable() => EventBus.Unsubscribe<BaseCoreDestroyedEvent>(OnCoreDestroyed);

        public void StartLevel(LevelData level)
        {
            _level = level;
            _timeRemaining = Mathf.Max(20f, level.timeLimitSeconds);
            _noUnitsTimer = 0f;
            _battleActive = true;
            _currentBoss = null;

            LevelManager.Instance.LoadLevel(level);

            string cannonId = "scrapcannon_mk1";
            int cannonLevel = 1;
            if (ServiceLocator.TryGet<CannonManager>(out var cannonManager))
            {
                cannonId = cannonManager.EquippedCannonId;
                cannonLevel = cannonManager.GetLevel(cannonId);
            }

            GameDatabase.Instance.Cannons.TryGetValue(cannonId, out var cannonData);
            string startingUnitId = cannonData != null ? cannonData.spawnUnitId : "recruit";
            CrowdManager.Instance.ConfigureForLevel(level.battlefieldLength, startingUnitId, 1);

            if (level.startingUnits > 0)
            {
                CrowdManager.Instance.SpawnMany(level.startingUnits, BattlefieldBounds.Instance.CannonSpawnPoint, 1.5f);
            }

            CannonFireController.Instance.BeginBattle(cannonId, cannonLevel);

            if (ChampionManager.Instance != null)
            {
                ChampionManager.Instance.SpawnForBattle(BattlefieldBounds.Instance.CannonSpawnPoint + Vector3.forward * 1.5f);
            }

            if (level.isBossLevel && GameDatabase.Instance.Bosses.TryGetValue(level.bossId, out var bossData))
            {
                float difficultyScale = 1f + (level.difficulty - 1) * 0.12f;
                Vector3 bossPos = new Vector3(0f, 1f, level.battlefieldLength);
                _currentBoss = EnemyManager.Instance.SpawnBoss(bossData, bossPos, difficultyScale);
            }

            if (ServiceLocator.TryGet<Progression.MissionManager>(out var missions)) missions.NotifyBattleStarted();

            GameManager.Instance.ChangeState(GameState.Battle);
        }

        private void Update()
        {
            if (!_battleActive) return;
            float dt = Time.deltaTime;

            Gates.GateManager.Instance.Tick(dt);
            SpecialMechanics.ObstacleManager.Instance.Tick(dt);
            CombatSystem.Instance.Tick(dt);
            CrowdManager.Instance.Tick(dt);
            EnemyManager.Instance.Tick(dt);
            ChampionManager.Instance?.Tick(dt);
            _currentBoss?.TickBoss(dt);

            _timeRemaining -= dt;

            if (CrowdManager.Instance.Count == 0)
            {
                _noUnitsTimer += dt;
                if (_noUnitsTimer >= NoUnitsGraceSeconds)
                {
                    ResolveDefeat();
                    return;
                }
            }
            else
            {
                _noUnitsTimer = 0f;
            }

            if (_timeRemaining <= 0f)
            {
                ResolveDefeat();
                return;
            }

            if (_level.isBossLevel && _currentBoss != null && !_currentBoss.IsAlive)
            {
                EventBus.Publish(new BossDefeatedEvent(_level.bossId));
                ResolveVictory();
            }
        }

        private void OnCoreDestroyed(BaseCoreDestroyedEvent e)
        {
            if (!_battleActive || (_level != null && _level.isBossLevel)) return;
            ResolveVictory();
        }

        private void ResolveVictory()
        {
            if (!_battleActive) return;
            _battleActive = false;
            CannonFireController.Instance.StopFiring();
            EventBus.Publish(new BattleVictoryEvent(CrowdManager.Instance.Count));
            GameManager.Instance.ChangeState(GameState.Victory);
        }

        private void ResolveDefeat()
        {
            if (!_battleActive) return;
            _battleActive = false;
            CannonFireController.Instance.StopFiring();
            EventBus.Publish(new BattleDefeatEvent());
            GameManager.Instance.ChangeState(GameState.Defeat);
        }

        public LevelData CurrentLevel => _level;
        public float TimeRemaining => _timeRemaining;
    }
}
