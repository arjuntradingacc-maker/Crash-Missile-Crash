using System.Linq;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.Progression
{
    public readonly struct SeasonTierClaimedEvent : IGameEvent
    {
        public readonly int Tier; public readonly bool Premium;
        public SeasonTierClaimedEvent(int tier, bool premium) { Tier = tier; Premium = premium; }
    }

    /// <summary>Drives the current seasonal Free/Premium reward track.</summary>
    public class SeasonManager : MonoBehaviour
    {
        public static SeasonManager Instance { get; private set; }

        public int SeasonPoints { get; private set; }
        public bool HasPremiumPass { get; private set; }
        private readonly System.Collections.Generic.HashSet<int> _claimedFree = new System.Collections.Generic.HashSet<int>();
        private readonly System.Collections.Generic.HashSet<int> _claimedPremium = new System.Collections.Generic.HashSet<int>();

        public SeasonData CurrentSeason => GameDatabase.Instance.Seasons.Values.FirstOrDefault();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public void AddPoints(int amount)
        {
            if (amount > 0) SeasonPoints += amount;
        }

        /// <summary>Cumulative points required to reach a tier (sum of pointsRequired up to and including it).</summary>
        public int PointsRequiredForTier(int tier)
        {
            var season = CurrentSeason;
            if (season == null) return int.MaxValue;
            int total = 0;
            foreach (var t in season.tiers)
            {
                total += t.pointsRequired;
                if (t.tier == tier) return total;
            }
            return int.MaxValue;
        }

        public bool IsTierUnlocked(int tier) => SeasonPoints >= PointsRequiredForTier(tier);

        public bool TryPurchasePremiumPass()
        {
            var season = CurrentSeason;
            if (season == null || HasPremiumPass) return false;
            if (!ServiceLocator.TryGet<EconomyManager>(out var economy)) return false;
            if (!economy.TrySpend(CurrencyType.Gems, season.premiumPassGemCost)) return false;
            HasPremiumPass = true;
            return true;
        }

        public bool TryClaimTier(int tier, bool premium)
        {
            var season = CurrentSeason;
            if (season == null || !IsTierUnlocked(tier)) return false;
            if (premium && !HasPremiumPass) return false;

            var claimedSet = premium ? _claimedPremium : _claimedFree;
            if (!claimedSet.Add(tier)) return false;

            var reward = season.tiers.FirstOrDefault(t => t.tier == tier);
            if (reward == null) return false;

            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                economy.Add(CurrencyType.Coins, premium ? reward.premiumCoins : reward.freeCoins);
                economy.Add(CurrencyType.Gems, premium ? reward.premiumGems : reward.freeGems);
            }

            string cardId = premium ? reward.premiumCardId : reward.freeCardId;
            int cardCount = premium ? reward.premiumCardCount : reward.freeCardCount;
            if (!string.IsNullOrEmpty(cardId) && ServiceLocator.TryGet<CardManager>(out var cards))
            {
                cards.AddCards(cardId, cardCount);
            }

            if (premium && !string.IsNullOrEmpty(reward.premiumCosmeticKey) && ServiceLocator.TryGet<InventoryManager>(out var inventory))
            {
                inventory.GrantCosmetic(reward.premiumCosmeticKey);
            }

            EventBus.Publish(new SeasonTierClaimedEvent(tier, premium));
            return true;
        }

        public void LoadState(int points, bool premium)
        {
            SeasonPoints = points;
            HasPremiumPass = premium;
        }
    }
}
