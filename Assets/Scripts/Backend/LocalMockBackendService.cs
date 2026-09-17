using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrashMissileCrash.Base;
using UnityEngine;

namespace CrashMissileCrash.Backend
{
    /// <summary>
    /// Fully offline stand-in for IBackendService. Cloud save is written to local disk instead of
    /// a server, raid "opponents" are synthesized procedurally instead of matched against real
    /// players, and purchase validation always succeeds. This keeps every online-feature code
    /// path (raids, leaderboards, IAP) exercised and testable without a backend deployment; swap
    /// this class out for a real implementation to go live with true async PvP and server-
    /// validated purchases.
    /// </summary>
    public class LocalMockBackendService : IBackendService
    {
        private readonly System.Random _rng = new System.Random();
        private readonly List<RaidRecord> _syntheticIncomingAttacks = new List<RaidRecord>();

        public Task<string> AuthenticateAsync()
        {
            string id = PlayerPrefs.GetString("cmc_player_id", string.Empty);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString("cmc_player_id", id);
                PlayerPrefs.Save();
            }
            return Task.FromResult(id);
        }

        public Task<CloudSaveBlob> DownloadSaveAsync(string playerId)
        {
            string key = "cmc_cloud_save_" + playerId;
            string payload = PlayerPrefs.GetString(key, string.Empty);
            var blob = new CloudSaveBlob { PlayerId = playerId, JsonPayload = payload, LastUpdatedUtc = DateTime.UtcNow };
            return Task.FromResult(blob);
        }

        public Task UploadSaveAsync(string playerId, string jsonPayload)
        {
            PlayerPrefs.SetString("cmc_cloud_save_" + playerId, jsonPayload);
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }

        public Task<OpponentProfile> FindRaidOpponentAsync(int attackerTrophies)
        {
            var names = new[] { "Ferrostrike", "Vantablade", "Cindermaw", "Nullpoint", "Graveshift", "Ashborn" };
            var opponent = new OpponentProfile
            {
                PlayerId = "bot_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                DisplayName = names[_rng.Next(names.Length)] + _rng.Next(10, 99),
                Trophies = Mathf.Max(0, attackerTrophies + _rng.Next(-150, 150)),
                BuildingLevels = new Dictionary<BuildingType, int>()
            };
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                opponent.BuildingLevels[type] = Mathf.Max(1, Mathf.RoundToInt(attackerTrophies / 300f) + _rng.Next(-1, 2));
            }
            return Task.FromResult(opponent);
        }

        public Task<RaidRecord> SubmitRaidResultAsync(string opponentId, bool attackerWon, int coinsStolen, int trophyChange)
        {
            var record = new RaidRecord
            {
                RaidId = Guid.NewGuid().ToString("N"),
                AttackerId = "local_player",
                AttackerName = "You",
                AttackerWon = attackerWon,
                CoinsStolen = coinsStolen,
                TrophyChange = trophyChange,
                TimestampUtc = DateTime.UtcNow
            };

            // Simulate an occasional revenge-able incoming attack from "another player".
            if (_rng.NextDouble() < 0.3)
            {
                _syntheticIncomingAttacks.Add(new RaidRecord
                {
                    RaidId = Guid.NewGuid().ToString("N"),
                    AttackerId = opponentId,
                    AttackerName = "Rival Commander",
                    AttackerWon = _rng.NextDouble() < 0.5,
                    CoinsStolen = _rng.Next(50, 400),
                    TrophyChange = -_rng.Next(5, 30),
                    TimestampUtc = DateTime.UtcNow
                });
            }

            return Task.FromResult(record);
        }

        public Task<List<RaidRecord>> GetIncomingAttacksAsync(string playerId)
        {
            return Task.FromResult(new List<RaidRecord>(_syntheticIncomingAttacks));
        }

        public Task<List<(string playerId, string displayName, int score)>> GetLeaderboardAsync(string leaderboardId, int topN)
        {
            var result = new List<(string, string, int)>();
            for (int i = 0; i < topN; i++)
            {
                result.Add(($"bot_{i}", $"Commander{i:000}", 5000 - i * 73));
            }
            return Task.FromResult(result);
        }

        public Task<bool> ValidatePurchaseAsync(string productId, string receiptPayload)
        {
            // A real backend must verify the receipt with Apple/Google server-to-server here.
            return Task.FromResult(true);
        }
    }
}
