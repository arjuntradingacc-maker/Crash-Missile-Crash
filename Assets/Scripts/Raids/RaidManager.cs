using System.Threading.Tasks;
using CrashMissileCrash.Backend;
using CrashMissileCrash.Base;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using CrashMissileCrash.Progression;
using UnityEngine;

namespace CrashMissileCrash.Raids
{
    public readonly struct RaidResolvedEvent : IGameEvent
    {
        public readonly bool Won; public readonly int CoinsStolen; public readonly int TrophyChange; public readonly string OpponentName;
        public RaidResolvedEvent(bool won, int coinsStolen, int trophyChange, string opponentName)
        {
            Won = won; CoinsStolen = coinsStolen; TrophyChange = trophyChange; OpponentName = opponentName;
        }
    }

    /// <summary>
    /// Asynchronous PvP: attacks a saved opponent defense layout (fetched through IBackendService)
    /// rather than requiring a live opponent session. Outcome is a simplified power comparison -
    /// a production backend would run (or validate) the actual simulated battle server-side to
    /// prevent client-side result tampering.
    /// </summary>
    public class RaidManager : MonoBehaviour
    {
        public static RaidManager Instance { get; private set; }

        private IBackendService _backend;
        public OpponentProfile LastOpponent { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _backend);
        }

        public async Task<OpponentProfile> FindOpponentAsync()
        {
            int trophies = ServiceLocator.TryGet<LeagueManager>(out var league) ? league.Trophies : 0;
            LastOpponent = await _backend.FindRaidOpponentAsync(trophies);
            return LastOpponent;
        }

        public async Task<RaidResolvedEvent> AttackAsync(OpponentProfile opponent)
        {
            var layout = new DefenseLayout { BuildingLevels = new System.Collections.Generic.Dictionary<BuildingType, int>(opponent.BuildingLevels) };
            int defensePower = layout.TotalDefensePower();
            int attackPower = ComputeAttackPower();

            bool won = attackPower >= defensePower * Random.Range(0.75f, 1.1f);
            int coinsStolen = won ? Mathf.RoundToInt(opponent.Trophies * Random.Range(0.8f, 1.4f)) + 100 : 0;
            int trophyChange = won ? Random.Range(15, 35) : -Random.Range(5, 20);

            await _backend.SubmitRaidResultAsync(opponent.PlayerId, won, coinsStolen, trophyChange);

            if (ServiceLocator.TryGet<EconomyManager>(out var economy)) economy.Add(CurrencyType.Coins, coinsStolen);
            if (ServiceLocator.TryGet<LeagueManager>(out var league)) league.AddTrophies(trophyChange);
            if (won && ServiceLocator.TryGet<MissionManager>(out var missions)) missions.NotifyRaidWon();

            var result = new RaidResolvedEvent(won, coinsStolen, trophyChange, opponent.DisplayName);
            EventBus.Publish(result);
            return result;
        }

        private int ComputeAttackPower()
        {
            int trophies = ServiceLocator.TryGet<LeagueManager>(out var league) ? league.Trophies : 0;
            int playerLevel = ServiceLocator.TryGet<ProgressionManager>(out var progression) ? progression.PlayerLevel : 1;
            return Mathf.RoundToInt(trophies / 30f) + playerLevel * 5 + 20;
        }
    }
}
