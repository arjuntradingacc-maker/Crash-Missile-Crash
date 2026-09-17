using System;

namespace CrashMissileCrash.Data
{
    public enum CardTargetType
    {
        Unit,
        Cannon,
        Champion
    }

    [Serializable]
    public class CardData
    {
        public string id;
        public CardTargetType targetType;
        public string targetId;       // references UnitData/CannonData/ChampionData id
        public Rarity rarity = Rarity.Common;
        /// <summary>How many duplicate cards + coins are required to go from level N to N+1. Index = level-1.</summary>
        public int[] upgradeCardCosts = { 2, 4, 10, 20, 50, 100, 200, 400, 800, 1600 };
        public int[] upgradeCoinCosts = { 50, 150, 400, 1000, 2500, 6000, 15000, 35000, 80000, 200000 };
    }

    [Serializable]
    public class CardPackData
    {
        public string id;
        public string displayName;
        public int cardCount = 3;
        public float commonWeight = 70f;
        public float rareWeight = 22f;
        public float epicWeight = 6.5f;
        public float legendaryWeight = 1.4f;
        public float mythicWeight = 0.1f;
        public int coinCost;
        public int gemCost;
        public float openDurationSeconds = 2.5f;
    }
}
