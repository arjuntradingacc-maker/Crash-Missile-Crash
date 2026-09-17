using System.Collections.Generic;
using System.Threading.Tasks;
using CrashMissileCrash.Backend;
using CrashMissileCrash.Base;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Raids
{
    public readonly struct IncomingAttackNotifiedEvent : IGameEvent
    {
        public readonly RaidRecord Record;
        public IncomingAttackNotifiedEvent(RaidRecord record) { Record = record; }
    }

    /// <summary>Surfaces incoming attack notifications and lets the player launch a revenge raid.</summary>
    public class RevengeManager : MonoBehaviour
    {
        public static RevengeManager Instance { get; private set; }

        private IBackendService _backend;
        private readonly List<RaidRecord> _incoming = new List<RaidRecord>();
        public IReadOnlyList<RaidRecord> IncomingAttacks => _incoming;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private void Start() => ServiceLocator.TryGet(out _backend);

        public async Task RefreshIncomingAttacksAsync(string playerId)
        {
            var records = await _backend.GetIncomingAttacksAsync(playerId);
            foreach (var r in records)
            {
                if (_incoming.Exists(x => x.RaidId == r.RaidId)) continue;
                _incoming.Add(r);
                EventBus.Publish(new IncomingAttackNotifiedEvent(r));
            }
        }

        public async Task<RaidResolvedEvent> LaunchRevengeAsync(RaidRecord record)
        {
            var opponent = new OpponentProfile
            {
                PlayerId = record.AttackerId,
                DisplayName = record.AttackerName,
                Trophies = Mathf.Abs(record.TrophyChange) * 20,
                BuildingLevels = new Dictionary<BuildingType, int>()
            };
            foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
            {
                opponent.BuildingLevels[type] = Mathf.Max(1, Mathf.Abs(record.TrophyChange) / 10);
            }

            var result = await RaidManager.Instance.AttackAsync(opponent);
            _incoming.Remove(record);
            return result;
        }
    }
}
