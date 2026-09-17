using System.Collections.Generic;
using CrashMissileCrash.Ads;
using CrashMissileCrash.Base;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Cannons;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using CrashMissileCrash.Events;
using CrashMissileCrash.IAP;
using CrashMissileCrash.Progression;
using CrashMissileCrash.Raids;
using UnityEngine;

namespace CrashMissileCrash.Analytics
{
    /// <summary>
    /// Wires the game's telemetry surface. Every manager fires plain gameplay events on EventBus;
    /// this class is the only place that knows those events double as analytics - add a new
    /// tracked event here without touching the system that raised it.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }
        private IAnalyticsProvider _provider;
        private float _sessionStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            _provider = new ConsoleAnalyticsProvider(); // swap for Firebase/GameAnalytics/etc. to go live
            _sessionStartTime = Time.unscaledTime;
        }

        private void Start() => Log(AnalyticsEvents.GameStarted, null);

        private void OnEnable()
        {
            EventBus.Subscribe<BattleVictoryEvent>(OnBattleVictory);
            EventBus.Subscribe<BattleDefeatEvent>(OnBattleDefeat);
            EventBus.Subscribe<GateTriggeredEvent>(OnGateTriggered);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<CardUpgradedEvent>(OnCardUpgraded);
            EventBus.Subscribe<CardsGrantedEvent>(OnCardsGranted);
            EventBus.Subscribe<CannonUnlockedEvent>(OnCannonUnlocked);
            EventBus.Subscribe<ChampionUnlockedEvent>(OnChampionUnlocked);
            EventBus.Subscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
            EventBus.Subscribe<RaidResolvedEvent>(OnRaidResolved);
            EventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
            EventBus.Subscribe<SeasonTierClaimedEvent>(OnSeasonTierClaimed);
            EventBus.Subscribe<RewardedAdStartedEvent>(OnRewardedAdStarted);
            EventBus.Subscribe<RewardedAdCompletedEvent>(OnRewardedAdCompleted);
            EventBus.Subscribe<PurchaseStartedEvent>(OnPurchaseStarted);
            EventBus.Subscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BattleVictoryEvent>(OnBattleVictory);
            EventBus.Unsubscribe<BattleDefeatEvent>(OnBattleDefeat);
            EventBus.Unsubscribe<GateTriggeredEvent>(OnGateTriggered);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<CardUpgradedEvent>(OnCardUpgraded);
            EventBus.Unsubscribe<CardsGrantedEvent>(OnCardsGranted);
            EventBus.Unsubscribe<CannonUnlockedEvent>(OnCannonUnlocked);
            EventBus.Unsubscribe<ChampionUnlockedEvent>(OnChampionUnlocked);
            EventBus.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
            EventBus.Unsubscribe<RaidResolvedEvent>(OnRaidResolved);
            EventBus.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
            EventBus.Unsubscribe<SeasonTierClaimedEvent>(OnSeasonTierClaimed);
            EventBus.Unsubscribe<RewardedAdStartedEvent>(OnRewardedAdStarted);
            EventBus.Unsubscribe<RewardedAdCompletedEvent>(OnRewardedAdCompleted);
            EventBus.Unsubscribe<PurchaseStartedEvent>(OnPurchaseStarted);
            EventBus.Unsubscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);

            Log(AnalyticsEvents.SessionLength, new Dictionary<string, object> { { "seconds", Time.unscaledTime - _sessionStartTime } });
        }

        public void Log(string eventName, Dictionary<string, object> parameters) => _provider.LogEvent(eventName, parameters);

        private void OnBattleVictory(BattleVictoryEvent e) => Log(AnalyticsEvents.BattleWon, new Dictionary<string, object> { { "survivors", e.SurvivingUnits } });
        private void OnBattleDefeat(BattleDefeatEvent e) => Log(AnalyticsEvents.BattleLost, null);
        private void OnGateTriggered(GateTriggeredEvent e) => Log(AnalyticsEvents.GateUsed, new Dictionary<string, object> { { "label", e.Label } });
        private void OnBossDefeated(BossDefeatedEvent e) => Log(AnalyticsEvents.BossDefeated, new Dictionary<string, object> { { "bossId", e.BossId } });
        private void OnCardUpgraded(CardUpgradedEvent e) => Log(AnalyticsEvents.CardUpgraded, new Dictionary<string, object> { { "cardId", e.CardId }, { "level", e.NewLevel } });
        private void OnCardsGranted(CardsGrantedEvent e) => Log(AnalyticsEvents.CardUnlocked, new Dictionary<string, object> { { "cardId", e.CardId } });
        private void OnCannonUnlocked(CannonUnlockedEvent e) => Log(AnalyticsEvents.CannonUnlocked, new Dictionary<string, object> { { "cannonId", e.CannonId } });
        private void OnChampionUnlocked(ChampionUnlockedEvent e) => Log(AnalyticsEvents.ChampionUnlocked, new Dictionary<string, object> { { "championId", e.ChampionId } });
        private void OnBuildingUpgraded(BuildingUpgradedEvent e) => Log(AnalyticsEvents.BaseUpgraded, new Dictionary<string, object> { { "building", e.Building.ToString() } });
        private void OnRaidResolved(RaidResolvedEvent e) => Log(e.Won ? AnalyticsEvents.RaidWon : AnalyticsEvents.RaidLost, null);
        private void OnMissionCompleted(MissionCompletedEvent e) => Log(AnalyticsEvents.MissionCompleted, new Dictionary<string, object> { { "missionId", e.MissionId } });
        private void OnSeasonTierClaimed(SeasonTierClaimedEvent e) => Log(AnalyticsEvents.SeasonTierCompleted, new Dictionary<string, object> { { "tier", e.Tier } });
        private void OnRewardedAdStarted(RewardedAdStartedEvent e) => Log(AnalyticsEvents.RewardedAdStarted, null);
        private void OnRewardedAdCompleted(RewardedAdCompletedEvent e) => Log(AnalyticsEvents.RewardedAdCompleted, new Dictionary<string, object> { { "rewarded", e.Rewarded } });
        private void OnPurchaseStarted(PurchaseStartedEvent e) => Log(AnalyticsEvents.PurchaseStarted, new Dictionary<string, object> { { "productId", e.ProductId } });
        private void OnPurchaseCompleted(PurchaseCompletedEvent e) => Log(AnalyticsEvents.PurchaseCompleted, new Dictionary<string, object> { { "productId", e.ProductId }, { "success", e.Success } });
    }
}
