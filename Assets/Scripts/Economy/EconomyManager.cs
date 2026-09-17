using System.Collections.Generic;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Economy
{
    public readonly struct CurrencyChangedEvent : IGameEvent
    {
        public readonly CurrencyType Currency; public readonly int NewBalance; public readonly int Delta;
        public CurrencyChangedEvent(CurrencyType currency, int newBalance, int delta) { Currency = currency; NewBalance = newBalance; Delta = delta; }
    }

    /// <summary>
    /// Single source of truth for every currency balance. All spends/grants across the game
    /// (battle rewards, card upgrades, shop, missions, season rewards, raids) flow through here
    /// so nothing can silently desync the player's economy. In production, server-side purchase
    /// validation (see IAPManager/IBackendService) must confirm any real-money-derived grant
    /// before it is applied here - never trust a client-only currency change for paid content.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        private readonly Dictionary<CurrencyType, int> _balances = new Dictionary<CurrencyType, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);

            foreach (CurrencyType t in System.Enum.GetValues(typeof(CurrencyType))) _balances[t] = 0;
            if (GameDatabase.Instance != null)
            {
                foreach (var starting in GameDatabase.Instance.Economy.startingBalances)
                {
                    _balances[starting.currency] = starting.amount;
                }
            }
        }

        public int GetBalance(CurrencyType currency) => _balances.TryGetValue(currency, out var v) ? v : 0;

        public void Add(CurrencyType currency, int amount)
        {
            if (amount <= 0) return;
            _balances.TryGetValue(currency, out var current);
            _balances[currency] = current + amount;
            EventBus.Publish(new CurrencyChangedEvent(currency, _balances[currency], amount));
        }

        public bool TrySpend(CurrencyType currency, int amount)
        {
            if (amount <= 0) return true;
            if (GetBalance(currency) < amount) return false;
            _balances[currency] -= amount;
            EventBus.Publish(new CurrencyChangedEvent(currency, _balances[currency], -amount));
            return true;
        }

        public IReadOnlyDictionary<CurrencyType, int> AllBalances => _balances;

        public void LoadState(Dictionary<CurrencyType, int> balances)
        {
            foreach (var kv in balances) _balances[kv.Key] = kv.Value;
        }
    }
}
