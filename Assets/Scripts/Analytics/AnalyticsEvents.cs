namespace CrashMissileCrash.Analytics
{
    /// <summary>Canonical event name constants so every call site agrees on spelling.</summary>
    public static class AnalyticsEvents
    {
        public const string GameStarted = "game_started";
        public const string TutorialStarted = "tutorial_started";
        public const string TutorialCompleted = "tutorial_completed";
        public const string BattleStarted = "battle_started";
        public const string BattleWon = "battle_won";
        public const string BattleLost = "battle_lost";
        public const string GateUsed = "gate_used";
        public const string BossStarted = "boss_started";
        public const string BossDefeated = "boss_defeated";
        public const string CardUnlocked = "card_unlocked";
        public const string CardUpgraded = "card_upgraded";
        public const string CannonUnlocked = "cannon_unlocked";
        public const string ChampionUnlocked = "champion_unlocked";
        public const string ChampionUpgraded = "champion_upgraded";
        public const string BaseUpgraded = "base_upgraded";
        public const string RaidStarted = "raid_started";
        public const string RaidWon = "raid_won";
        public const string RaidLost = "raid_lost";
        public const string MissionCompleted = "mission_completed";
        public const string EventStarted = "event_started";
        public const string EventCompleted = "event_completed";
        public const string SeasonStarted = "season_started";
        public const string SeasonTierCompleted = "season_tier_completed";
        public const string RewardedAdStarted = "rewarded_ad_started";
        public const string RewardedAdCompleted = "rewarded_ad_completed";
        public const string PurchaseStarted = "purchase_started";
        public const string PurchaseCompleted = "purchase_completed";
        public const string SessionLength = "session_length";
    }
}
