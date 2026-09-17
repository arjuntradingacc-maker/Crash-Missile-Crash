using System;

namespace CrashMissileCrash.Data
{
    public enum ChampionArchetype
    {
        Tank,
        Berserker,
        Commander,
        Ninja,
        Engineer,
        Mage
    }

    public enum UltimateAbilityType
    {
        AreaSlam,       // shockwave / area attack
        LightningStrike,
        ClonePlatoon,   // temporary cloning
        FrostNova,      // freeze
        RallyBoost,     // speed/damage buff to nearby units
        Titan           // giant transformation
    }

    [Serializable]
    public class ChampionData
    {
        public string id;
        public string displayName;
        public ChampionArchetype archetype = ChampionArchetype.Commander;
        public Rarity rarity = Rarity.Epic;
        public CombatStatsData baseStats = new CombatStatsData();
        public CombatStatsData perLevelGrowth = new CombatStatsData();
        public int maxLevel = 20;
        public string visualKey;

        public UltimateAbilityType ultimateType = UltimateAbilityType.AreaSlam;
        public float ultimateCooldown = 20f;
        public float ultimatePower = 10f;     // damage/heal/duration scalar, meaning depends on ability
        public float ultimateRadius = 4f;
        public float passiveValue;             // e.g. % damage aura, meaning depends on archetype
        public string passiveDescriptionKey;
    }
}
