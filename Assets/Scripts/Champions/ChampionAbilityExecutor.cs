using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Champions
{
    public readonly struct ChampionUltimateFiredEvent : IGameEvent
    {
        public readonly Vector3 Position; public readonly UltimateAbilityType Type;
        public ChampionUltimateFiredEvent(Vector3 position, UltimateAbilityType type) { Position = position; Type = type; }
    }

    /// <summary>Applies the visually-impressive effect for each champion ultimate archetype.</summary>
    public static class ChampionAbilityExecutor
    {
        public static void Execute(ChampionUnit champion)
        {
            var data = champion.Data;
            Vector3 origin = champion.transform.position;
            EventBus.Publish(new ChampionUltimateFiredEvent(origin, data.ultimateType));

            switch (data.ultimateType)
            {
                case UltimateAbilityType.AreaSlam:
                    DamageEnemiesInRadius(origin, data.ultimateRadius, data.ultimatePower, int.MaxValue);
                    break;

                case UltimateAbilityType.LightningStrike:
                    DamageEnemiesInRadius(origin, data.ultimateRadius, data.ultimatePower, 4);
                    break;

                case UltimateAbilityType.ClonePlatoon:
                    if (CrowdManager.Instance != null)
                    {
                        CrowdManager.Instance.SpawnMany(Mathf.RoundToInt(data.ultimatePower), origin, 2.5f);
                    }
                    break;

                case UltimateAbilityType.FrostNova:
                    FreezeEnemiesInRadius(origin, data.ultimateRadius, data.ultimatePower);
                    break;

                case UltimateAbilityType.RallyBoost:
                    if (CrowdManager.Instance != null)
                    {
                        CrowdManager.Instance.SpawnMany(Mathf.RoundToInt(data.ultimatePower), origin, 2.5f);
                    }
                    break;

                case UltimateAbilityType.Titan:
                    champion.Stats.Damage *= 1f + data.ultimatePower;
                    champion.Stats.MaxHealth *= 1f + data.ultimatePower * 0.5f;
                    champion.Stats.Health = Mathf.Min(champion.Stats.MaxHealth, champion.Stats.Health * (1f + data.ultimatePower * 0.5f));
                    break;
            }
        }

        private static void DamageEnemiesInRadius(Vector3 origin, float radius, float damage, int maxTargets)
        {
            if (EnemyManager.Instance == null) return;
            int hit = 0;
            foreach (var enemy in EnemyManager.Instance.ActiveUnits)
            {
                if (hit >= maxTargets) break;
                if (!enemy.IsAlive) continue;
                if (Vector3.Distance(origin, enemy.transform.position) > radius) continue;
                enemy.TakeDamage(damage, false);
                hit++;
            }
        }

        private static void FreezeEnemiesInRadius(Vector3 origin, float radius, float freezeSeconds)
        {
            if (EnemyManager.Instance == null) return;
            foreach (var enemy in EnemyManager.Instance.ActiveUnits)
            {
                if (!enemy.IsAlive) continue;
                if (Vector3.Distance(origin, enemy.transform.position) > radius) continue;
                enemy.TakeDamage(freezeSeconds * 2f, false);
                enemy.AttackTimer = Mathf.Max(enemy.AttackTimer, freezeSeconds);
            }
        }
    }
}
