using System.Collections.Generic;
using System.Linq;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Data
{
    /// <summary>
    /// Central, in-memory registry of all data-driven game content, loaded once at boot from
    /// Resources/Data/*.json. Every manager (battle, cards, cannons, champions, economy,
    /// missions, events, seasons, shop) resolves content by string id through this database
    /// instead of holding direct references, so new content can be added purely as data.
    /// </summary>
    public class GameDatabase : MonoBehaviour
    {
        public static GameDatabase Instance { get; private set; }

        public Dictionary<string, UnitData> Units { get; private set; } = new Dictionary<string, UnitData>();
        public Dictionary<string, EnemyData> Enemies { get; private set; } = new Dictionary<string, EnemyData>();
        public Dictionary<string, BossData> Bosses { get; private set; } = new Dictionary<string, BossData>();
        public Dictionary<string, ChampionData> Champions { get; private set; } = new Dictionary<string, ChampionData>();
        public Dictionary<string, CannonData> Cannons { get; private set; } = new Dictionary<string, CannonData>();
        public Dictionary<string, CardData> Cards { get; private set; } = new Dictionary<string, CardData>();
        public Dictionary<string, CardPackData> CardPacks { get; private set; } = new Dictionary<string, CardPackData>();
        public Dictionary<string, GateDefinitionData> Gates { get; private set; } = new Dictionary<string, GateDefinitionData>();
        public Dictionary<string, WorldData> Worlds { get; private set; } = new Dictionary<string, WorldData>();
        public Dictionary<string, LevelData> Levels { get; private set; } = new Dictionary<string, LevelData>();
        public List<string> LevelIdsInOrder { get; private set; } = new List<string>();
        public Dictionary<string, MissionData> Missions { get; private set; } = new Dictionary<string, MissionData>();
        public Dictionary<string, LiveEventData> LiveEvents { get; private set; } = new Dictionary<string, LiveEventData>();
        public Dictionary<string, SeasonData> Seasons { get; private set; } = new Dictionary<string, SeasonData>();
        public Dictionary<string, ShopOfferData> ShopOffers { get; private set; } = new Dictionary<string, ShopOfferData>();
        public List<LeagueDivisionConfig> LeagueDivisions { get; private set; } = new List<LeagueDivisionConfig>();
        public EconomyConfigData Economy { get; private set; } = new EconomyConfigData();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            LoadAll();
        }

        private void LoadAll()
        {
            Units = DataLoader.LoadJson<UnitDataCollection>("Data/units").items.ToDictionary(u => u.id);
            Enemies = DataLoader.LoadJson<EnemyDataCollection>("Data/enemies").items.ToDictionary(e => e.id);
            Bosses = DataLoader.LoadJson<BossDataCollection>("Data/bosses").items.ToDictionary(b => b.id);
            Champions = DataLoader.LoadJson<ChampionDataCollection>("Data/champions").items.ToDictionary(c => c.id);
            Cannons = DataLoader.LoadJson<CannonDataCollection>("Data/cannons").items.ToDictionary(c => c.id);
            Cards = DataLoader.LoadJson<CardDataCollection>("Data/cards").items.ToDictionary(c => c.id);
            CardPacks = DataLoader.LoadJson<CardPackDataCollection>("Data/card_packs").items.ToDictionary(c => c.id);
            Gates = DataLoader.LoadJson<GateDefinitionCollection>("Data/gates").items.ToDictionary(g => g.id);
            Worlds = DataLoader.LoadJson<WorldDataCollection>("Data/worlds").items.ToDictionary(w => w.id);
            Missions = DataLoader.LoadJson<MissionDataCollection>("Data/missions").items.ToDictionary(m => m.id);
            LiveEvents = DataLoader.LoadJson<LiveEventDataCollection>("Data/events").items.ToDictionary(e => e.id);
            Seasons = DataLoader.LoadJson<SeasonDataCollection>("Data/seasons").items.ToDictionary(s => s.id);
            ShopOffers = DataLoader.LoadJson<ShopOfferCollection>("Data/shop").items.ToDictionary(s => s.id);
            LeagueDivisions = DataLoader.LoadJson<LeagueDivisionCollection>("Data/leagues").items;
            Economy = DataLoader.LoadJson<EconomyConfigData>("Data/economy_config");

            var levelIndex = DataLoader.LoadJson<LevelIndexData>("Data/level_index");
            foreach (var levelPath in levelIndex.levelResourcePaths)
            {
                var level = DataLoader.LoadLevel(levelPath);
                if (level == null) continue;
                Levels[level.id] = level;
                LevelIdsInOrder.Add(level.id);
            }

            Debug.Log($"[GameDatabase] Loaded {Units.Count} units, {Enemies.Count} enemies, {Cannons.Count} cannons, " +
                      $"{Champions.Count} champions, {Cards.Count} cards, {Worlds.Count} worlds, {Levels.Count} levels.");
        }

        public LevelData GetNextLevel(string currentLevelId)
        {
            int idx = LevelIdsInOrder.IndexOf(currentLevelId);
            if (idx < 0 || idx + 1 >= LevelIdsInOrder.Count) return null;
            return Levels[LevelIdsInOrder[idx + 1]];
        }
    }

    [System.Serializable]
    public class LevelIndexData
    {
        public List<string> levelResourcePaths = new List<string>();
    }
}
