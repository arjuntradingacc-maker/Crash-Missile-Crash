using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Data
{
    [Serializable]
    public class CurrencyStartingBalance
    {
        public CurrencyType currency;
        public int amount;
    }

    [Serializable]
    public class XpLevelEntry
    {
        public int level;
        public int xpRequired;
    }

    [Serializable]
    public class EconomyConfigData
    {
        public List<CurrencyStartingBalance> startingBalances = new List<CurrencyStartingBalance>();
        public List<XpLevelEntry> xpCurve = new List<XpLevelEntry>();
        public float interstitialMinSecondsBetween = 120f;
        public float rewardedAdCoinMultiplier = 2f;
    }
}
