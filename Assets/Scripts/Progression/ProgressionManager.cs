using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Progression
{
    public readonly struct PlayerLeveledUpEvent : IGameEvent
    {
        public readonly int NewLevel;
        public PlayerLeveledUpEvent(int newLevel) { NewLevel = newLevel; }
    }

    /// <summary>Tracks the player's account XP/level using the data-driven xp curve.</summary>
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public int Xp { get; private set; }
        public int PlayerLevel { get; private set; } = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            Xp += amount;

            var curve = GameDatabase.Instance.Economy.xpCurve;
            bool leveledUp = false;
            foreach (var entry in curve)
            {
                if (entry.level > PlayerLevel && Xp >= entry.xpRequired)
                {
                    PlayerLevel = entry.level;
                    leveledUp = true;
                }
            }
            if (leveledUp) EventBus.Publish(new PlayerLeveledUpEvent(PlayerLevel));
        }

        public void LoadState(int xp, int level)
        {
            Xp = xp;
            PlayerLevel = level;
        }
    }
}
