using System;

namespace CrashMissileCrash.Data
{
    public enum MissionCadence
    {
        Daily,
        Weekly,
        Achievement,
        Milestone
    }

    public enum MissionMetric
    {
        BattlesStarted,
        BattlesWon,
        UnitsMultiplied,
        BasesDestroyed,
        CardsUpgraded,
        CoinsEarned,
        BossesDefeated,
        GatesUsed,
        RaidsWon
    }

    [Serializable]
    public class MissionData
    {
        public string id;
        public MissionCadence cadence = MissionCadence.Daily;
        public MissionMetric metric = MissionMetric.BattlesWon;
        public int targetValue = 3;
        public int rewardCoins;
        public int rewardGems;
        public int rewardXp;
        public string rewardCardId;
        public int rewardCardCount;
        public string descriptionKey; // localization key, e.g. "mission.battles_won"
    }
}
