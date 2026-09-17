using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Data
{
    public enum LiveEventFormat
    {
        BossRush,
        TimeAttack,
        EndlessBattle,
        TreasureHunt,
        CollectionChallenge,
        SpecialBattle,
        LimitedTimeWorld,
        Tournament
    }

    [Serializable]
    public class EventRewardTier
    {
        public int scoreThreshold;
        public int coins;
        public int gems;
        public string cardId;
        public int cardCount;
    }

    [Serializable]
    public class LiveEventData
    {
        public string id;
        public string displayName;
        public LiveEventFormat format = LiveEventFormat.SpecialBattle;
        public string startTimeUtcIso;
        public string endTimeUtcIso;
        public int minPlayerLevel;
        public string specialCurrencyId; // e.g. "event_token"
        public float difficultyModifier = 1f;
        public List<EventRewardTier> rewardTiers = new List<EventRewardTier>();
        public List<string> levelIds = new List<string>(); // event-specific levels, can reuse world levels
        public bool hasLeaderboard = true;
    }
}
