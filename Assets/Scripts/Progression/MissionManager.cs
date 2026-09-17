using System;
using System.Collections.Generic;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.Progression
{
    public readonly struct MissionProgressedEvent : IGameEvent
    {
        public readonly string MissionId; public readonly int Current; public readonly int Target;
        public MissionProgressedEvent(string missionId, int current, int target) { MissionId = missionId; Current = current; Target = target; }
    }

    public readonly struct MissionCompletedEvent : IGameEvent
    {
        public readonly string MissionId;
        public MissionCompletedEvent(string missionId) { MissionId = missionId; }
    }

    /// <summary>
    /// Tracks progress toward every daily/weekly/achievement/milestone mission by listening to
    /// gameplay events - no gameplay code needs to know missions exist. New missions are added
    /// purely as data (Resources/Data/missions.json) mapped to one of the MissionMetric counters.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        private readonly Dictionary<string, int> _progress = new Dictionary<string, int>();
        private readonly HashSet<string> _claimed = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<BattleVictoryEvent>(OnBattleVictory);
            EventBus.Subscribe<GateTriggeredEvent>(OnGateTriggered);
            EventBus.Subscribe<BaseCoreDestroyedEvent>(OnBaseDestroyed);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<CardUpgradedEvent>(OnCardUpgraded);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BattleVictoryEvent>(OnBattleVictory);
            EventBus.Unsubscribe<GateTriggeredEvent>(OnGateTriggered);
            EventBus.Unsubscribe<BaseCoreDestroyedEvent>(OnBaseDestroyed);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<CardUpgradedEvent>(OnCardUpgraded);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        private void OnBattleVictory(BattleVictoryEvent e) => OnMetric(MissionMetric.BattlesWon, 1);
        private void OnBaseDestroyed(BaseCoreDestroyedEvent e) => OnMetric(MissionMetric.BasesDestroyed, 1);
        private void OnBossDefeated(BossDefeatedEvent e) => OnMetric(MissionMetric.BossesDefeated, 1);
        private void OnCardUpgraded(CardUpgradedEvent e) => OnMetric(MissionMetric.CardsUpgraded, 1);

        public void NotifyBattleStarted() => OnMetric(MissionMetric.BattlesStarted, 1);
        public void NotifyRaidWon() => OnMetric(MissionMetric.RaidsWon, 1);

        private void OnGateTriggered(GateTriggeredEvent e)
        {
            OnMetric(MissionMetric.GatesUsed, 1);
            int delta = e.CrowdCountAfter - e.CrowdCountBefore;
            if (delta > 0) OnMetric(MissionMetric.UnitsMultiplied, delta);
        }

        private void OnCurrencyChanged(CurrencyChangedEvent e)
        {
            if (e.Currency == CurrencyType.Coins && e.Delta > 0) OnMetric(MissionMetric.CoinsEarned, e.Delta);
        }

        private void OnMetric(MissionMetric metric, int amount)
        {
            foreach (var mission in GameDatabase.Instance.Missions.Values)
            {
                if (mission.metric != metric || amount == 0) continue;
                if (_claimed.Contains(mission.id)) continue;

                _progress.TryGetValue(mission.id, out var current);
                int next = Mathf.Min(mission.targetValue, current + amount);
                _progress[mission.id] = next;
                EventBus.Publish(new MissionProgressedEvent(mission.id, next, mission.targetValue));

                if (next >= mission.targetValue && current < mission.targetValue)
                {
                    EventBus.Publish(new MissionCompletedEvent(mission.id));
                }
            }
        }

        public int GetProgress(string missionId) => _progress.TryGetValue(missionId, out var v) ? v : 0;
        public bool IsComplete(string missionId) => GameDatabase.Instance.Missions.TryGetValue(missionId, out var m) && GetProgress(missionId) >= m.targetValue;
        public bool IsClaimed(string missionId) => _claimed.Contains(missionId);

        public bool TryClaim(string missionId)
        {
            if (!IsComplete(missionId) || IsClaimed(missionId)) return false;
            if (!GameDatabase.Instance.Missions.TryGetValue(missionId, out var mission)) return false;

            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                economy.Add(CurrencyType.Coins, mission.rewardCoins);
                economy.Add(CurrencyType.Gems, mission.rewardGems);
            }
            if (ServiceLocator.TryGet<ProgressionManager>(out var progression))
            {
                progression.AddXp(mission.rewardXp);
            }
            if (!string.IsNullOrEmpty(mission.rewardCardId) && ServiceLocator.TryGet<CardManager>(out var cards))
            {
                cards.AddCards(mission.rewardCardId, mission.rewardCardCount);
            }

            _claimed.Add(missionId);
            return true;
        }

        public IReadOnlyDictionary<string, int> AllProgress => _progress;
        public IReadOnlyCollection<string> AllClaimed => _claimed;

        public void LoadState(Dictionary<string, int> progress, IEnumerable<string> claimed)
        {
            _progress.Clear();
            foreach (var kv in progress) _progress[kv.Key] = kv.Value;
            _claimed.Clear();
            foreach (var id in claimed) _claimed.Add(id);
        }
    }
}
