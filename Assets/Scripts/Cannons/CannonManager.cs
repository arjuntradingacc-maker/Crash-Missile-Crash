using System.Collections.Generic;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Cannons
{
    public readonly struct CannonUnlockedEvent : IGameEvent
    {
        public readonly string CannonId;
        public CannonUnlockedEvent(string cannonId) { CannonId = cannonId; }
    }

    public readonly struct CannonEquippedEvent : IGameEvent
    {
        public readonly string CannonId;
        public CannonEquippedEvent(string cannonId) { CannonId = cannonId; }
    }

    /// <summary>Tracks which cannons are unlocked/equipped. Upgrade levels are delegated to CardManager
    /// (each cannon has a matching card) so there is only one upgrade-progression system in the game.</summary>
    public class CannonManager : MonoBehaviour
    {
        public static CannonManager Instance { get; private set; }

        private readonly HashSet<string> _unlocked = new HashSet<string>();
        public string EquippedCannonId { get; private set; } = "scrapcannon_mk1";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            _unlocked.Add("scrapcannon_mk1");
        }

        public bool IsUnlocked(string cannonId) => _unlocked.Contains(cannonId);

        public void Unlock(string cannonId)
        {
            if (_unlocked.Add(cannonId)) EventBus.Publish(new CannonUnlockedEvent(cannonId));
        }

        public bool TryEquip(string cannonId)
        {
            if (!IsUnlocked(cannonId)) return false;
            EquippedCannonId = cannonId;
            EventBus.Publish(new CannonEquippedEvent(cannonId));
            return true;
        }

        public int GetLevel(string cannonId)
        {
            return CardManager.Instance != null ? CardManager.Instance.GetLevelForTarget(CardTargetType.Cannon, cannonId) : 1;
        }

        public IReadOnlyCollection<string> UnlockedCannons => _unlocked;

        public void LoadState(IEnumerable<string> unlocked, string equipped)
        {
            _unlocked.Clear();
            foreach (var id in unlocked) _unlocked.Add(id);
            if (!string.IsNullOrEmpty(equipped)) EquippedCannonId = equipped;
        }
    }
}
