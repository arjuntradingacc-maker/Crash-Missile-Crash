using System;
using System.Collections.Generic;
using System.Linq;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.Events
{
    public readonly struct LiveEventScoreChangedEvent : IGameEvent
    {
        public readonly string EventId; public readonly int Score;
        public LiveEventScoreChangedEvent(string eventId, int score) { EventId = eventId; Score = score; }
    }

    /// <summary>
    /// Generic driver for time-boxed live events (Boss Rush, Time Attack, Endless Battle, Treasure
    /// Hunt, Collection Challenge, Tournament, ...). Every event is just LiveEventData - new event
    /// formats are added as data/content, this class only needs to know how to track score and
    /// hand out the matching reward tier, which is format-agnostic.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private readonly Dictionary<string, int> _scores = new Dictionary<string, int>();
        private readonly Dictionary<string, HashSet<int>> _claimedTiers = new Dictionary<string, HashSet<int>>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public IEnumerable<LiveEventData> GetActiveEvents()
        {
            var now = DateTime.UtcNow;
            foreach (var e in GameDatabase.Instance.LiveEvents.Values)
            {
                if (DateTime.TryParse(e.startTimeUtcIso, out var start) && DateTime.TryParse(e.endTimeUtcIso, out var end))
                {
                    if (now >= start && now <= end) yield return e;
                }
            }
        }

        public bool IsEventActive(string eventId) => GetActiveEvents().Any(e => e.id == eventId);

        public int GetScore(string eventId) => _scores.TryGetValue(eventId, out var s) ? s : 0;

        public void AddScore(string eventId, int amount)
        {
            if (amount <= 0 || !IsEventActive(eventId)) return;
            _scores.TryGetValue(eventId, out var current);
            _scores[eventId] = current + amount;
            EventBus.Publish(new LiveEventScoreChangedEvent(eventId, _scores[eventId]));
        }

        public bool TryClaimTier(string eventId, int tierIndex)
        {
            if (!GameDatabase.Instance.LiveEvents.TryGetValue(eventId, out var evt)) return false;
            if (tierIndex < 0 || tierIndex >= evt.rewardTiers.Count) return false;
            var tier = evt.rewardTiers[tierIndex];
            if (GetScore(eventId) < tier.scoreThreshold) return false;

            if (!_claimedTiers.TryGetValue(eventId, out var claimed))
            {
                claimed = new HashSet<int>();
                _claimedTiers[eventId] = claimed;
            }
            if (!claimed.Add(tierIndex)) return false;

            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                economy.Add(CurrencyType.Coins, tier.coins);
                economy.Add(CurrencyType.Gems, tier.gems);
            }
            if (!string.IsNullOrEmpty(tier.cardId) && ServiceLocator.TryGet<CardManager>(out var cards))
            {
                cards.AddCards(tier.cardId, tier.cardCount);
            }
            return true;
        }

        public IReadOnlyDictionary<string, int> AllScores => _scores;

        public void LoadState(Dictionary<string, int> scores)
        {
            _scores.Clear();
            foreach (var kv in scores) _scores[kv.Key] = kv.Value;
        }
    }
}
