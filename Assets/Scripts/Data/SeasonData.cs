using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Data
{
    [Serializable]
    public class SeasonTierReward
    {
        public int tier;
        public int freeCoins;
        public int freeGems;
        public string freeCardId;
        public int freeCardCount;
        public int premiumCoins;
        public int premiumGems;
        public string premiumCardId;
        public int premiumCardCount;
        public string premiumCosmeticKey;
        public int pointsRequired = 100;
    }

    [Serializable]
    public class SeasonData
    {
        public string id;
        public string displayName;
        public string themeKey;
        public string startTimeUtcIso;
        public string endTimeUtcIso;
        public int premiumPassGemCost = 500;
        public List<SeasonTierReward> tiers = new List<SeasonTierReward>();
    }

    public enum LeagueDivision
    {
        Rookie,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        Champion
    }

    [Serializable]
    public class LeagueDivisionConfig
    {
        public LeagueDivision division;
        public int trophiesToPromote;
        public int trophiesToDemote;
        public int seasonRewardCoins;
        public int seasonRewardGems;
    }
}
