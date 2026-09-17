using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrashMissileCrash.Base;

namespace CrashMissileCrash.Backend
{
    [Serializable]
    public class OpponentProfile
    {
        public string PlayerId;
        public string DisplayName;
        public int Trophies;
        public Dictionary<BuildingType, int> BuildingLevels = new Dictionary<BuildingType, int>();
    }

    [Serializable]
    public class RaidRecord
    {
        public string RaidId;
        public string AttackerId;
        public string AttackerName;
        public bool AttackerWon;
        public int CoinsStolen;
        public int TrophyChange;
        public DateTime TimestampUtc;
    }

    [Serializable]
    public class CloudSaveBlob
    {
        public string PlayerId;
        public string JsonPayload;
        public DateTime LastUpdatedUtc;
    }

    /// <summary>
    /// Everything that needs a live server in production: auth, cloud save, raid matchmaking/
    /// records, leaderboards and IAP receipt validation. LocalMockBackendService implements this
    /// entirely offline so the game is fully playable without a server; swapping in a real
    /// implementation (Firebase, PlayFab, a bespoke service, etc.) requires touching only that
    /// one class - every manager that needs the network talks to IBackendService, never a
    /// concrete provider.
    /// </summary>
    public interface IBackendService
    {
        Task<string> AuthenticateAsync();
        Task<CloudSaveBlob> DownloadSaveAsync(string playerId);
        Task UploadSaveAsync(string playerId, string jsonPayload);

        Task<OpponentProfile> FindRaidOpponentAsync(int attackerTrophies);
        Task<RaidRecord> SubmitRaidResultAsync(string opponentId, bool attackerWon, int coinsStolen, int trophyChange);
        Task<List<RaidRecord>> GetIncomingAttacksAsync(string playerId);

        Task<List<(string playerId, string displayName, int score)>> GetLeaderboardAsync(string leaderboardId, int topN);

        /// <summary>Never trust a client-reported purchase; a real backend must verify the receipt
        /// with Apple/Google server-to-server before granting any product.</summary>
        Task<bool> ValidatePurchaseAsync(string productId, string receiptPayload);
    }
}
