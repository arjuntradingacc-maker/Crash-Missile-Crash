using System;

namespace CrashMissileCrash.Data
{
    public enum EnemyBehaviorType
    {
        Melee,
        Ranged,
        Tank,
        Fast,
        Defensive,
        Support,
        Special,
        Boss
    }

    [Serializable]
    public class EnemyData
    {
        public string id;
        public string displayName;
        public EnemyBehaviorType behavior = EnemyBehaviorType.Melee;
        public CombatStatsData baseStats = new CombatStatsData();
        public string visualKey;
        public string specialAbilityId; // e.g. "shield_ally", "slow_aura", "spawn_minions"
        public int coinDropMin = 1;
        public int coinDropMax = 3;
    }

    [Serializable]
    public class BossPhaseData
    {
        public float healthThreshold01 = 1f; // phase begins at this fraction of boss HP
        public string abilityId;
        public float abilityCooldown = 8f;
        public float damageMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
    }

    [Serializable]
    public class BossData
    {
        public string id;
        public string displayName;
        public CombatStatsData baseStats = new CombatStatsData();
        public string visualKey;
        public System.Collections.Generic.List<BossPhaseData> phases = new System.Collections.Generic.List<BossPhaseData>();
        public string minionEnemyId;
        public int minionWaveSize = 4;
        public float minionSpawnInterval = 12f;
        public float enrageTimeSeconds = 90f;
        public float enrageDamageMultiplier = 1.5f;
    }
}
