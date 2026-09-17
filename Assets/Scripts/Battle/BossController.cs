using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Drives a boss encounter: large health pool, phase transitions at health thresholds,
    /// periodic special abilities, minion waves and an enrage state if the fight runs too long.
    /// Reuses the same targeting path as regular enemies (ICombatTarget via EnemyUnit) so the
    /// crowd auto-engages it identically once in range.
    /// </summary>
    public class BossController : EnemyUnit
    {
        public BossData Data { get; private set; }
        private int _phaseIndex;
        private float _abilityTimer;
        private float _minionTimer;
        private float _fightTimer;
        private bool _enraged;

        public void InitializeBoss(BossData data, Vector3 position, float difficultyScale)
        {
            Data = data;
            var stats = CombatStatsRuntime.FromData(data.baseStats, new CombatStatsData(), 1);
            stats.MaxHealth *= difficultyScale;
            stats.Health = stats.MaxHealth;
            stats.Damage *= difficultyScale;
            ResetForSpawn(stats, position);
            EnemyDataId = "boss_" + data.id;
            DetectionRange = 999f; // bosses always aggro immediately
            VisualKey = data.visualKey;

            _phaseIndex = 0;
            _abilityTimer = data.phases.Count > 0 ? data.phases[0].abilityCooldown : 10f;
            _minionTimer = data.minionSpawnInterval;
            _fightTimer = 0f;
            _enraged = false;
        }

        public void TickBoss(float deltaTime)
        {
            if (!IsAlive) return;
            _fightTimer += deltaTime;

            UpdatePhase();

            if (!_enraged && Data.enrageTimeSeconds > 0f && _fightTimer >= Data.enrageTimeSeconds)
            {
                _enraged = true;
                Stats.Damage *= Data.enrageDamageMultiplier;
                EventBus.Publish(new BossEnragedEvent(transform.position));
            }

            _abilityTimer -= deltaTime;
            if (_abilityTimer <= 0f)
            {
                var phase = Data.phases[_phaseIndex];
                _abilityTimer = phase.abilityCooldown;
                EventBus.Publish(new BossAbilityUsedEvent(transform.position, phase.abilityId));
            }

            if (!string.IsNullOrEmpty(Data.minionEnemyId))
            {
                _minionTimer -= deltaTime;
                if (_minionTimer <= 0f)
                {
                    _minionTimer = Data.minionSpawnInterval;
                    SpawnMinionWave();
                }
            }
        }

        private void UpdatePhase()
        {
            if (Data.phases.Count == 0) return;
            float healthFraction = Stats.Health / Mathf.Max(1f, Stats.MaxHealth);

            for (int i = Data.phases.Count - 1; i >= 0; i--)
            {
                if (healthFraction <= Data.phases[i].healthThreshold01 && i > _phaseIndex)
                {
                    _phaseIndex = i;
                    var phase = Data.phases[i];
                    Stats.Damage *= phase.damageMultiplier;
                    Stats.MoveSpeed *= phase.moveSpeedMultiplier;
                    EventBus.Publish(new BossPhaseChangedEvent(transform.position, i));
                    break;
                }
            }
        }

        private void SpawnMinionWave()
        {
            if (EnemyManager.Instance == null || BattlefieldBounds.Instance == null) return;
            var bounds = BattlefieldBounds.Instance;
            for (int i = 0; i < Data.minionWaveSize; i++)
            {
                float x = bounds.ClampX(transform.position.x + (i - Data.minionWaveSize * 0.5f) * 1.2f);
                var pos = new Vector3(x, 0.5f, Mathf.Max(0f, transform.position.z - 3f));
                EnemyManager.Instance.SpawnEnemy(Data.minionEnemyId, pos, 1f);
            }
        }
    }

    public readonly struct BossPhaseChangedEvent : IGameEvent
    {
        public readonly Vector3 Position; public readonly int PhaseIndex;
        public BossPhaseChangedEvent(Vector3 position, int phaseIndex) { Position = position; PhaseIndex = phaseIndex; }
    }

    public readonly struct BossAbilityUsedEvent : IGameEvent
    {
        public readonly Vector3 Position; public readonly string AbilityId;
        public BossAbilityUsedEvent(Vector3 position, string abilityId) { Position = position; AbilityId = abilityId; }
    }

    public readonly struct BossEnragedEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public BossEnragedEvent(Vector3 position) { Position = position; }
    }
}
