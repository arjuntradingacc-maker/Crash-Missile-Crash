using System.Collections.Generic;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.Cards
{
    public readonly struct CardsGrantedEvent : IGameEvent
    {
        public readonly string CardId; public readonly int Count;
        public CardsGrantedEvent(string cardId, int count) { CardId = cardId; Count = count; }
    }

    public readonly struct CardUpgradedEvent : IGameEvent
    {
        public readonly string CardId; public readonly int NewLevel;
        public CardUpgradedEvent(string cardId, int newLevel) { CardId = cardId; NewLevel = newLevel; }
    }

    /// <summary>
    /// Owns the player's card collection and upgrade levels for every unit/cannon/champion.
    /// CrowdManager, CannonManager and ChampionManager all read levels through here so there is
    /// a single source of truth for "how strong is X right now".
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance { get; private set; }

        private readonly Dictionary<string, int> _cardCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public int GetCardCount(string cardId) => _cardCounts.TryGetValue(cardId, out var c) ? c : 0;

        public int GetLevel(string cardId) => _levels.TryGetValue(cardId, out var l) ? l : 1;

        public void AddCards(string cardId, int count)
        {
            if (count <= 0 || string.IsNullOrEmpty(cardId)) return;
            _cardCounts.TryGetValue(cardId, out var current);
            _cardCounts[cardId] = current + count;
            EventBus.Publish(new CardsGrantedEvent(cardId, count));
        }

        public bool CanUpgrade(string cardId)
        {
            if (!GameDatabase.Instance.Cards.TryGetValue(cardId, out var card)) return false;
            int level = GetLevel(cardId);
            int index = level - 1;
            if (index >= card.upgradeCardCosts.Length) return false;

            int cardCost = card.upgradeCardCosts[index];
            int coinCost = index < card.upgradeCoinCosts.Length ? card.upgradeCoinCosts[index] : 0;

            bool hasCards = GetCardCount(cardId) >= cardCost;
            bool hasCoins = !ServiceLocator.TryGet<EconomyManager>(out var economy) || economy.GetBalance(CurrencyType.Coins) >= coinCost;
            return hasCards && hasCoins;
        }

        public bool TryUpgrade(string cardId)
        {
            if (!CanUpgrade(cardId)) return false;
            var card = GameDatabase.Instance.Cards[cardId];
            int level = GetLevel(cardId);
            int index = level - 1;
            int cardCost = card.upgradeCardCosts[index];
            int coinCost = index < card.upgradeCoinCosts.Length ? card.upgradeCoinCosts[index] : 0;

            _cardCounts[cardId] -= cardCost;
            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                economy.TrySpend(CurrencyType.Coins, coinCost);
            }

            _levels[cardId] = level + 1;
            EventBus.Publish(new CardUpgradedEvent(cardId, level + 1));
            return true;
        }

        /// <summary>Resolves the gameplay level for a target (unit/cannon/champion) via its card id.</summary>
        public int GetLevelForTarget(CardTargetType type, string targetId)
        {
            foreach (var kvp in GameDatabase.Instance.Cards)
            {
                if (kvp.Value.targetType == type && kvp.Value.targetId == targetId)
                {
                    return GetLevel(kvp.Key);
                }
            }
            return 1;
        }

        public IReadOnlyDictionary<string, int> AllCardCounts => _cardCounts;
        public IReadOnlyDictionary<string, int> AllLevels => _levels;

        public void LoadState(Dictionary<string, int> counts, Dictionary<string, int> levels)
        {
            _cardCounts.Clear();
            foreach (var kv in counts) _cardCounts[kv.Key] = kv.Value;
            _levels.Clear();
            foreach (var kv in levels) _levels[kv.Key] = kv.Value;
        }
    }
}
