using CrashMissileCrash.Data;

namespace CrashMissileCrash.Battle
{
    /// <summary>Mutable, per-instance combat stats resolved from data + upgrade level at spawn time.</summary>
    public class CombatStatsRuntime
    {
        public float MaxHealth;
        public float Health;
        public float Damage;
        public float Defense;
        public float AttackSpeed;
        public float MoveSpeed;
        public float Range;
        public float CritChance;
        public float CritMultiplier;

        public bool IsAlive => Health > 0f;

        public static CombatStatsRuntime FromData(CombatStatsData baseStats, CombatStatsData growth, int level)
        {
            int extraLevels = Mathf0(level - 1);
            var stats = new CombatStatsRuntime
            {
                MaxHealth = baseStats.health + growth.health * extraLevels,
                Damage = baseStats.damage + growth.damage * extraLevels,
                Defense = baseStats.defense + growth.defense * extraLevels,
                AttackSpeed = baseStats.attackSpeed + growth.attackSpeed * extraLevels,
                MoveSpeed = baseStats.moveSpeed + growth.moveSpeed * extraLevels,
                Range = baseStats.range + growth.range * extraLevels,
                CritChance = baseStats.critChance + growth.critChance * extraLevels,
                CritMultiplier = baseStats.critMultiplier + growth.critMultiplier * extraLevels
            };
            stats.Health = stats.MaxHealth;
            return stats;
        }

        private static int Mathf0(int v) => v < 0 ? 0 : v;

        /// <summary>Applies incoming damage after defense mitigation. Returns actual damage dealt.</summary>
        public float ApplyDamage(float incomingDamage)
        {
            float mitigated = UnityEngine.Mathf.Max(1f, incomingDamage - Defense);
            Health = UnityEngine.Mathf.Max(0f, Health - mitigated);
            return mitigated;
        }

        public float RollDamage(System.Random rng)
        {
            bool isCrit = rng.NextDouble() < CritChance;
            return isCrit ? Damage * CritMultiplier : Damage;
        }
    }
}
