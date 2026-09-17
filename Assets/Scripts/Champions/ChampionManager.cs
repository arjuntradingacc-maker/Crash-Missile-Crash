using System.Collections.Generic;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Champions
{
    public readonly struct ChampionUnlockedEvent : IGameEvent
    {
        public readonly string ChampionId;
        public ChampionUnlockedEvent(string championId) { ChampionId = championId; }
    }

    /// <summary>Tracks unlocked champions and the one deployed into battle.</summary>
    public class ChampionManager : MonoBehaviour
    {
        public static ChampionManager Instance { get; private set; }

        private readonly HashSet<string> _unlocked = new HashSet<string>();
        public string DeployedChampionId { get; private set; } = "ironclad";
        public ChampionUnit ActiveChampionUnit { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            _unlocked.Add("ironclad");
        }

        public bool IsUnlocked(string championId) => _unlocked.Contains(championId);
        public IReadOnlyCollection<string> UnlockedChampions => _unlocked;

        public void Unlock(string championId)
        {
            if (_unlocked.Add(championId)) EventBus.Publish(new ChampionUnlockedEvent(championId));
        }

        public bool TryDeploy(string championId)
        {
            if (!IsUnlocked(championId)) return false;
            DeployedChampionId = championId;
            return true;
        }

        public int GetLevel(string championId)
        {
            return CardManager.Instance != null ? CardManager.Instance.GetLevelForTarget(CardTargetType.Champion, championId) : 1;
        }

        public void SpawnForBattle(Vector3 position)
        {
            DespawnCurrent();
            if (string.IsNullOrEmpty(DeployedChampionId)) return;
            if (!GameDatabase.Instance.Champions.TryGetValue(DeployedChampionId, out var data)) return;

            var template = VisualRegistry.Instance.GetOrCreateTemplate(
                data.visualKey, PlaceholderShape.Capsule, PrimitiveFactory.ColorForRarity((int)data.rarity), new Vector3(0.9f, 0.9f, 0.9f));
            var go = Object.Instantiate(template);
            go.SetActive(true);
            var unit = go.AddComponent<ChampionUnit>();
            unit.InitializeChampion(data, GetLevel(DeployedChampionId), position);
            ActiveChampionUnit = unit;
        }

        public void Tick(float deltaTime)
        {
            if (ActiveChampionUnit == null || BattlefieldBounds.Instance == null) return;
            ActiveChampionUnit.TickMovement(deltaTime, BattlefieldBounds.Instance.Length);
        }

        public void DespawnCurrent()
        {
            if (ActiveChampionUnit != null)
            {
                Object.Destroy(ActiveChampionUnit.gameObject);
                ActiveChampionUnit = null;
            }
        }

        public void LoadState(IEnumerable<string> unlocked, string deployed)
        {
            _unlocked.Clear();
            foreach (var id in unlocked) _unlocked.Add(id);
            if (!string.IsNullOrEmpty(deployed)) DeployedChampionId = deployed;
        }
    }
}
