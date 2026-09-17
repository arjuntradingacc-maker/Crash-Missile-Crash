using System.Collections.Generic;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>Builds and owns the enemy base (defensive towers + central core) for the current level.</summary>
    public class EnemyBase : MonoBehaviour
    {
        public static EnemyBase Instance { get; private set; }

        private readonly List<BaseTower> _towers = new List<BaseTower>();
        public EnemyBaseCore Core { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Rebuild(LevelData level, float difficultyScale)
        {
            foreach (var tower in _towers)
            {
                if (tower != null) Destroy(tower.gameObject);
            }
            _towers.Clear();

            if (Core != null) Destroy(Core.gameObject);

            var bounds = BattlefieldBounds.Instance;
            Vector3 corePos = new Vector3(0f, 1f, level.battlefieldLength);

            var coreGo = PrimitiveFactory.CreatePlaceholder("EnemyBaseCore", PlaceholderShape.Cube, new Color(0.55f, 0.1f, 0.15f), new Vector3(2.2f, 2.2f, 2.2f));
            coreGo.transform.position = corePos;
            Core = coreGo.AddComponent<EnemyBaseCore>();
            Core.Initialize(level.baseHealth * difficultyScale);

            int towerCount = Mathf.Max(0, level.baseTowerCount);
            for (int i = 0; i < towerCount; i++)
            {
                float t = towerCount == 1 ? 0f : (i / (float)(towerCount - 1)) * 2f - 1f;
                float x = bounds.LaneToWorldX(t * 0.8f);
                Vector3 pos = new Vector3(bounds.ClampX(x), 0.6f, level.battlefieldLength - 4f);

                var towerGo = PrimitiveFactory.CreatePlaceholder($"BaseTower_{i}", PlaceholderShape.Cylinder, new Color(0.35f, 0.15f, 0.4f), new Vector3(0.8f, 1.2f, 0.8f));
                towerGo.transform.position = pos;
                var tower = towerGo.AddComponent<BaseTower>();
                var stats = new CombatStatsRuntime
                {
                    MaxHealth = level.baseTowerHealth * difficultyScale,
                    Health = level.baseTowerHealth * difficultyScale,
                    Damage = level.baseTowerDamage * difficultyScale,
                    Defense = 2f,
                    AttackSpeed = 0.8f,
                    MoveSpeed = 0f,
                    Range = 4.5f,
                    CritChance = 0f,
                    CritMultiplier = 1f
                };
                tower.ResetForSpawn(stats, pos);
                tower.DetectionRange = 6.5f;
                _towers.Add(tower);
            }
        }

        /// <summary>All currently damageable base targets (alive towers first, then the core).</summary>
        public IEnumerable<ICombatTarget> GetTargets()
        {
            foreach (var tower in _towers)
            {
                if (tower != null && tower.IsAlive) yield return tower;
            }
            if (Core != null && Core.IsAlive) yield return Core;
        }

        public IReadOnlyList<BaseTower> Towers => _towers;

        /// <summary>Lets ObstacleManager register mid-lane EnemyCannon/DefensiveTower obstacles so
        /// CombatSystem targets them the same way as base towers, without duplicating targeting code.</summary>
        public void RegisterExternalTower(BaseTower tower) => _towers.Add(tower);

        public void ClearAll()
        {
            foreach (var tower in _towers) if (tower != null) Destroy(tower.gameObject);
            _towers.Clear();
            if (Core != null) { Destroy(Core.gameObject); Core = null; }
        }
    }
}
