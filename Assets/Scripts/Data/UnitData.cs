using System;

namespace CrashMissileCrash.Data
{
    /// <summary>Base combat stat block shared by friendly units, enemies and champions.</summary>
    [Serializable]
    public class CombatStatsData
    {
        public float health = 10f;
        public float damage = 2f;
        public float defense = 0f;
        public float attackSpeed = 1f;   // attacks per second
        public float moveSpeed = 2.5f;   // meters/sec
        public float range = 1.2f;       // meters
        public float critChance = 0.05f;
        public float critMultiplier = 1.5f;
    }

    [Serializable]
    public class UnitData
    {
        public string id;
        public string displayName;
        public Rarity rarity = Rarity.Common;
        public CombatStatsData baseStats = new CombatStatsData();
        /// <summary>Additive per-level growth applied on top of baseStats by CardManager upgrades.</summary>
        public CombatStatsData perLevelGrowth = new CombatStatsData();
        public int maxLevel = 10;
        public string visualKey;     // key into an asset registry / addressable label - no hardcoded prefab refs
        public string specialAbilityId; // optional, e.g. "splash_attack", "heal_aura"
        public int unlockCardsRequired = 1;
    }
}
