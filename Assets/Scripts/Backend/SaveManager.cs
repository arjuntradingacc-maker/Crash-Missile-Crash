using System;
using System.Collections.Generic;
using System.IO;
using CrashMissileCrash.Base;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Cannons;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using CrashMissileCrash.Events;
using CrashMissileCrash.Progression;
using UnityEngine;

namespace CrashMissileCrash.Backend
{
    public readonly struct GameLoadedEvent : IGameEvent { }
    public readonly struct GameSavedEvent : IGameEvent { }

    /// <summary>
    /// Robust local-first save/load with best-effort cloud sync. Local save is authoritative for
    /// offline play; corrupted or missing save files fail safe into a fresh profile instead of
    /// crashing. Autosaves on key progress events (debounced) and on pause/quit so an interrupted
    /// battle or app kill never loses more than a few seconds of progress.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
        private IBackendService _backend;
        private string _playerId;

        private bool _dirty;
        private float _autosaveTimer;
        private const float AutosaveInterval = 15f;

        public string CurrentLevelId { get; set; } = "tutorial_level";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private async void Start()
        {
            ServiceLocator.TryGet(out _backend);
            if (_backend != null) _playerId = await _backend.AuthenticateAsync();
            Load();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CurrencyChangedEvent>(MarkDirty);
            EventBus.Subscribe<CardUpgradedEvent>(MarkDirty);
            EventBus.Subscribe<CardsGrantedEvent>(MarkDirty);
            EventBus.Subscribe<BattleVictoryEvent>(MarkDirty);
            EventBus.Subscribe<BuildingUpgradedEvent>(MarkDirty);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CurrencyChangedEvent>(MarkDirty);
            EventBus.Unsubscribe<CardUpgradedEvent>(MarkDirty);
            EventBus.Unsubscribe<CardsGrantedEvent>(MarkDirty);
            EventBus.Unsubscribe<BattleVictoryEvent>(MarkDirty);
            EventBus.Unsubscribe<BuildingUpgradedEvent>(MarkDirty);
        }

        private void MarkDirty<T>(T e) where T : IGameEvent => _dirty = true;

        private void Update()
        {
            if (!_dirty) return;
            _autosaveTimer += Time.deltaTime;
            if (_autosaveTimer >= AutosaveInterval)
            {
                _autosaveTimer = 0f;
                Save();
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();

        public void Save()
        {
            var data = BuildSaveData();
            data.lastSavedUtcIso = DateTime.UtcNow.ToString("o");

            try
            {
                string json = JsonUtility.ToJson(data, true);
                string tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Copy(tempPath, SavePath, true);
                File.Delete(tempPath);
                _dirty = false;
                EventBus.Publish(new GameSavedEvent());

                if (_backend != null && !string.IsNullOrEmpty(_playerId))
                {
                    _ = _backend.UploadSaveAsync(_playerId, json); // best-effort, fire-and-forget
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }

        public void Load()
        {
            PlayerSaveData data = null;
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    data = JsonUtility.FromJson<PlayerSaveData>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Local save corrupted, starting fresh: {e.Message}");
                data = null;
            }

            data ??= new PlayerSaveData();
            ApplySaveData(data);
            CurrentLevelId = data.currentLevelId;
            EventBus.Publish(new GameLoadedEvent());
        }

        private PlayerSaveData BuildSaveData()
        {
            var data = new PlayerSaveData { currentLevelId = CurrentLevelId };

            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                foreach (var kv in economy.AllBalances) data.currencies.Add(new CurrencyIntPair { key = kv.Key, value = kv.Value });
            }
            if (ServiceLocator.TryGet<CardManager>(out var cards))
            {
                foreach (var kv in cards.AllCardCounts) data.cardCounts.Add(new StringIntPair { key = kv.Key, value = kv.Value });
                foreach (var kv in cards.AllLevels) data.cardLevels.Add(new StringIntPair { key = kv.Key, value = kv.Value });
            }
            if (ServiceLocator.TryGet<CannonManager>(out var cannons))
            {
                data.unlockedCannons.AddRange(cannons.UnlockedCannons);
                data.equippedCannon = cannons.EquippedCannonId;
            }
            if (ServiceLocator.TryGet<ChampionManager>(out var champions))
            {
                data.unlockedChampions.AddRange(champions.UnlockedChampions);
                data.deployedChampion = champions.DeployedChampionId;
            }
            if (ServiceLocator.TryGet<ProgressionManager>(out var progression))
            {
                data.xp = progression.Xp;
                data.playerLevel = progression.PlayerLevel;
            }
            if (ServiceLocator.TryGet<MissionManager>(out var missions))
            {
                foreach (var kv in missions.AllProgress) data.missionProgress.Add(new StringIntPair { key = kv.Key, value = kv.Value });
                data.missionClaimed.AddRange(missions.AllClaimed);
            }
            if (ServiceLocator.TryGet<LeagueManager>(out var league))
            {
                data.trophies = league.Trophies;
            }
            if (ServiceLocator.TryGet<SeasonManager>(out var season))
            {
                data.seasonPoints = season.SeasonPoints;
                data.hasPremiumPass = season.HasPremiumPass;
            }
            if (ServiceLocator.TryGet<BaseManager>(out var baseManager))
            {
                foreach (var kv in baseManager.BuildDefenseSnapshot().BuildingLevels) data.buildingLevels.Add(new BuildingIntPair { key = kv.Key, value = kv.Value });
                data.shieldExpiresAtUtcIso = baseManager.ShieldExpiresAtUtc.ToString("o");
            }
            if (ServiceLocator.TryGet<EventManager>(out var events))
            {
                foreach (var kv in events.AllScores) data.eventScores.Add(new StringIntPair { key = kv.Key, value = kv.Value });
            }
            if (ServiceLocator.TryGet<InventoryManager>(out var inventory))
            {
                data.ownedCosmetics.AddRange(inventory.OwnedCosmetics);
            }

            return data;
        }

        private void ApplySaveData(PlayerSaveData data)
        {
            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                var dict = new Dictionary<CurrencyType, int>();
                foreach (var p in data.currencies) dict[p.key] = p.value;
                economy.LoadState(dict);
            }
            if (ServiceLocator.TryGet<CardManager>(out var cards))
            {
                var counts = new Dictionary<string, int>();
                foreach (var p in data.cardCounts) counts[p.key] = p.value;
                var levels = new Dictionary<string, int>();
                foreach (var p in data.cardLevels) levels[p.key] = p.value;
                cards.LoadState(counts, levels);
            }
            if (ServiceLocator.TryGet<CannonManager>(out var cannons))
            {
                cannons.LoadState(data.unlockedCannons, data.equippedCannon);
            }
            if (ServiceLocator.TryGet<ChampionManager>(out var champions))
            {
                champions.LoadState(data.unlockedChampions, data.deployedChampion);
            }
            if (ServiceLocator.TryGet<ProgressionManager>(out var progression))
            {
                progression.LoadState(data.xp, Mathf.Max(1, data.playerLevel));
            }
            if (ServiceLocator.TryGet<MissionManager>(out var missions))
            {
                var progress = new Dictionary<string, int>();
                foreach (var p in data.missionProgress) progress[p.key] = p.value;
                missions.LoadState(progress, data.missionClaimed);
            }
            if (ServiceLocator.TryGet<LeagueManager>(out var league))
            {
                league.LoadState(data.trophies);
            }
            if (ServiceLocator.TryGet<SeasonManager>(out var season))
            {
                season.LoadState(data.seasonPoints, data.hasPremiumPass);
            }
            if (ServiceLocator.TryGet<BaseManager>(out var baseManager))
            {
                var levels = new Dictionary<BuildingType, int>();
                foreach (var p in data.buildingLevels) levels[p.key] = p.value;
                DateTime.TryParse(data.shieldExpiresAtUtcIso, out var shieldExpiry);
                baseManager.LoadState(levels, shieldExpiry);
            }
            if (ServiceLocator.TryGet<EventManager>(out var events))
            {
                var scores = new Dictionary<string, int>();
                foreach (var p in data.eventScores) scores[p.key] = p.value;
                events.LoadState(scores);
            }
            if (ServiceLocator.TryGet<InventoryManager>(out var inventory))
            {
                inventory.LoadState(data.ownedCosmetics);
            }
        }
    }
}
