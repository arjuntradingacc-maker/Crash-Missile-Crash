using System;
using System.Collections.Generic;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.Base
{
    public enum BuildingType
    {
        Headquarters,
        DefenseTower,
        ResourceGenerator,
        TrainingCenter,
        CannonTower,
        ShieldGenerator,
        CardWorkshop
    }

    public readonly struct BuildingUpgradedEvent : IGameEvent
    {
        public readonly BuildingType Building; public readonly int NewLevel;
        public BuildingUpgradedEvent(BuildingType building, int newLevel) { Building = building; NewLevel = newLevel; }
    }

    public readonly struct ShieldActivatedEvent : IGameEvent
    {
        public readonly DateTime ExpiresAtUtc;
        public ShieldActivatedEvent(DateTime expiresAtUtc) { ExpiresAtUtc = expiresAtUtc; }
    }

    /// <summary>
    /// Owns the player's persistent home base: building levels (each visually/functionally
    /// improves as it levels up) and the temporary shield that protects it from raids.
    /// The building levels here ARE the "defense layout" RaidManager/IBackendService snapshot
    /// and send to the backend for other players to raid asynchronously.
    /// </summary>
    public class BaseManager : MonoBehaviour
    {
        public static BaseManager Instance { get; private set; }

        private readonly Dictionary<BuildingType, int> _levels = new Dictionary<BuildingType, int>();
        public DateTime ShieldExpiresAtUtc { get; private set; } = DateTime.MinValue;
        public bool IsShieldActive => DateTime.UtcNow < ShieldExpiresAtUtc;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);

            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType))) _levels[type] = 1;
        }

        public int GetLevel(BuildingType type) => _levels.TryGetValue(type, out var l) ? l : 1;

        public int UpgradeCoinCost(BuildingType type) => 500 * GetLevel(type) * GetLevel(type);
        public int UpgradeMaterialCost(BuildingType type) => 20 * GetLevel(type);

        public bool TryUpgrade(BuildingType type)
        {
            if (!ServiceLocator.TryGet<EconomyManager>(out var economy)) return false;
            int coinCost = UpgradeCoinCost(type);
            int materialCost = UpgradeMaterialCost(type);
            if (economy.GetBalance(CurrencyType.Coins) < coinCost || economy.GetBalance(CurrencyType.Materials) < materialCost) return false;

            economy.TrySpend(CurrencyType.Coins, coinCost);
            economy.TrySpend(CurrencyType.Materials, materialCost);
            _levels[type] = GetLevel(type) + 1;
            EventBus.Publish(new BuildingUpgradedEvent(type, _levels[type]));
            return true;
        }

        public void ActivateShield(TimeSpan duration)
        {
            var expiry = DateTime.UtcNow + duration;
            if (expiry > ShieldExpiresAtUtc) ShieldExpiresAtUtc = expiry;
            EventBus.Publish(new ShieldActivatedEvent(ShieldExpiresAtUtc));
        }

        /// <summary>Snapshot sent to the backend so other players can raid this base asynchronously.</summary>
        public Raids.DefenseLayout BuildDefenseSnapshot()
        {
            var snapshot = new Raids.DefenseLayout { BuildingLevels = new Dictionary<BuildingType, int>(_levels) };
            return snapshot;
        }

        public void LoadState(Dictionary<BuildingType, int> levels, DateTime shieldExpiry)
        {
            foreach (var kv in levels) _levels[kv.Key] = kv.Value;
            ShieldExpiresAtUtc = shieldExpiry;
        }
    }
}
