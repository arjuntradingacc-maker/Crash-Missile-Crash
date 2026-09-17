using System.Collections.Generic;
using System.Linq;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.Cards
{
    public readonly struct PackOpenedEvent : IGameEvent
    {
        public readonly string PackId;
        public readonly List<(string cardId, int count, Rarity rarity)> Rewards;
        public PackOpenedEvent(string packId, List<(string, int, Rarity)> rewards) { PackId = packId; Rewards = rewards; }
    }

    /// <summary>Resolves card-pack purchases and rarity-weighted card rolls.</summary>
    public class CardPackManager : MonoBehaviour
    {
        public static CardPackManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        public bool TryOpenPack(string packId, out List<(string cardId, int count, Rarity rarity)> rewards)
        {
            rewards = new List<(string, int, Rarity)>();
            if (!GameDatabase.Instance.CardPacks.TryGetValue(packId, out var pack)) return false;

            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                if (pack.gemCost > 0 && !economy.TrySpend(CurrencyType.Gems, pack.gemCost)) return false;
                if (pack.coinCost > 0 && !economy.TrySpend(CurrencyType.Coins, pack.coinCost)) return false;
            }

            for (int i = 0; i < pack.cardCount; i++)
            {
                var rarity = RollRarity(pack);
                var cardId = PickRandomCardOfRarity(rarity);
                if (string.IsNullOrEmpty(cardId)) continue;

                CardManager.Instance.AddCards(cardId, 1);
                rewards.Add((cardId, 1, rarity));
            }

            EventBus.Publish(new PackOpenedEvent(packId, rewards));
            return true;
        }

        private static Rarity RollRarity(CardPackData pack)
        {
            float total = pack.commonWeight + pack.rareWeight + pack.epicWeight + pack.legendaryWeight + pack.mythicWeight;
            float roll = Random.value * total;
            if ((roll -= pack.commonWeight) < 0) return Rarity.Common;
            if ((roll -= pack.rareWeight) < 0) return Rarity.Rare;
            if ((roll -= pack.epicWeight) < 0) return Rarity.Epic;
            if ((roll -= pack.legendaryWeight) < 0) return Rarity.Legendary;
            return Rarity.Mythic;
        }

        private static string PickRandomCardOfRarity(Rarity rarity)
        {
            var candidates = GameDatabase.Instance.Cards.Values.Where(c => c.rarity == rarity).ToList();
            if (candidates.Count == 0)
            {
                candidates = GameDatabase.Instance.Cards.Values.OrderBy(c => System.Math.Abs((int)c.rarity - (int)rarity)).ToList();
            }
            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)].id : null;
        }
    }
}
