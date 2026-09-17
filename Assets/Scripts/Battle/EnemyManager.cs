using System.Collections.Generic;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>Owns enemy pooling/spawning/movement for the current level.</summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        private readonly Dictionary<string, ObjectPool<EnemyUnit>> _poolsByEnemyId = new Dictionary<string, ObjectPool<EnemyUnit>>();
        private readonly List<EnemyUnit> _active = new List<EnemyUnit>();
        private readonly List<(EnemyUnit unit, float releaseAt)> _pendingRelease = new List<(EnemyUnit, float)>();

        public IReadOnlyList<EnemyUnit> ActiveUnits => _active;
        public int Count => _active.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private ObjectPool<EnemyUnit> GetOrCreatePool(EnemyData data)
        {
            if (_poolsByEnemyId.TryGetValue(data.id, out var pool)) return pool;

            var template = VisualRegistry.Instance.GetOrCreateTemplate(
                data.visualKey, PlaceholderShape.Cube, new Color(0.85f, 0.25f, 0.2f), new Vector3(0.6f, 0.6f, 0.6f));
            var prefabUnit = template.GetComponent<EnemyUnit>();
            if (prefabUnit == null) prefabUnit = template.AddComponent<EnemyUnit>();

            pool = new ObjectPool<EnemyUnit>(prefabUnit, transform, 16);
            _poolsByEnemyId[data.id] = pool;
            return pool;
        }

        public EnemyUnit SpawnEnemy(string enemyDataId, Vector3 position, float difficultyScale)
        {
            if (!GameDatabase.Instance.Enemies.TryGetValue(enemyDataId, out var data))
            {
                Debug.LogWarning($"[EnemyManager] Unknown enemy id: {enemyDataId}");
                return null;
            }

            var pool = GetOrCreatePool(data);
            var unit = pool.Get(position, Quaternion.identity);
            var stats = CombatStatsRuntime.FromData(data.baseStats, new CombatStatsData(), 1);
            stats.MaxHealth *= difficultyScale;
            stats.Health = stats.MaxHealth;
            stats.Damage *= difficultyScale;
            unit.ResetForSpawn(stats, position);
            unit.EnemyDataId = data.id;
            unit.DetectionRange = 6f;
            unit.VisualKey = data.visualKey;
            _active.Add(unit);
            return unit;
        }

        /// <summary>Registers a non-pooled unit (e.g. a boss) so CombatSystem/Tick treat it like any other enemy.</summary>
        public void RegisterExternalUnit(EnemyUnit unit) => _active.Add(unit);

        public BossController SpawnBoss(BossData data, Vector3 position, float difficultyScale)
        {
            var template = VisualRegistry.Instance.GetOrCreateTemplate(
                data.visualKey, PlaceholderShape.Cube, new Color(0.5f, 0.05f, 0.6f), new Vector3(2.2f, 2.6f, 2.2f));
            var go = Instantiate(template, transform);
            go.SetActive(true);
            var boss = go.AddComponent<BossController>();
            boss.InitializeBoss(data, position, difficultyScale);
            RegisterExternalUnit(boss);
            return boss;
        }

        public void SpawnLevelEnemies(LevelData level, float difficultyScale)
        {
            ClearAll();
            var bounds = BattlefieldBounds.Instance;
            foreach (var wave in level.enemyWaves)
            {
                for (int i = 0; i < wave.count; i++)
                {
                    float x = bounds.LaneToWorldX(wave.laneX) + (i - wave.count * 0.5f) * wave.spawnSpacing;
                    var pos = new Vector3(bounds.ClampX(x), 0.5f, wave.distanceFromStart);
                    SpawnEnemy(wave.enemyId, pos, difficultyScale);
                }
            }
        }

        private void ReleaseUnit(EnemyUnit unit)
        {
            _poolsByEnemyId[unit.EnemyDataId].Release(unit);
        }

        public void ClearAll()
        {
            foreach (var pool in _poolsByEnemyId.Values) pool.ReleaseAll();
            _active.Clear();
            _pendingRelease.Clear();
        }

        public void Tick(float deltaTime)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var unit = _active[i];
                if (!unit.IsAlive)
                {
                    _active.RemoveAt(i);
                    _pendingRelease.Add((unit, Time.time + 0.6f));
                    continue;
                }
                unit.TickMovement(deltaTime);
            }

            for (int i = _pendingRelease.Count - 1; i >= 0; i--)
            {
                if (Time.time >= _pendingRelease[i].releaseAt)
                {
                    ReleaseUnit(_pendingRelease[i].unit);
                    _pendingRelease.RemoveAt(i);
                }
            }
        }
    }
}
