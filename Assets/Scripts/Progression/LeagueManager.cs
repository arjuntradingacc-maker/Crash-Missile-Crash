using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Progression
{
    public readonly struct LeagueDivisionChangedEvent : IGameEvent
    {
        public readonly LeagueDivision NewDivision; public readonly bool Promoted;
        public LeagueDivisionChangedEvent(LeagueDivision newDivision, bool promoted) { NewDivision = newDivision; Promoted = promoted; }
    }

    /// <summary>Competitive trophy ladder across Rookie -> Champion divisions, fed by battle and raid results.</summary>
    public class LeagueManager : MonoBehaviour
    {
        public static LeagueManager Instance { get; private set; }

        public int Trophies { get; private set; }
        public LeagueDivision CurrentDivision { get; private set; } = LeagueDivision.Rookie;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private void OnEnable() => EventBus.Subscribe<BattleVictoryEvent>(OnBattleVictory);
        private void OnDisable() => EventBus.Unsubscribe<BattleVictoryEvent>(OnBattleVictory);

        private void OnBattleVictory(BattleVictoryEvent e)
        {
            int difficulty = BattleManager.Instance != null && BattleManager.Instance.CurrentLevel != null ? BattleManager.Instance.CurrentLevel.difficulty : 1;
            AddTrophies(Mathf.Max(3, difficulty * 4));
        }

        public void AddTrophies(int amount)
        {
            Trophies = Mathf.Max(0, Trophies + amount);
            RecalculateDivision(amount > 0);
        }

        private void RecalculateDivision(bool wasGain)
        {
            var divisions = GameDatabase.Instance.LeagueDivisions;
            if (divisions.Count == 0) return;

            var previous = CurrentDivision;
            int index = (int)CurrentDivision;

            while (index < divisions.Count - 1 && Trophies >= divisions[index].trophiesToPromote)
            {
                index++;
            }
            while (index > 0 && Trophies < divisions[index].trophiesToDemote)
            {
                index--;
            }

            CurrentDivision = (LeagueDivision)index;
            if (CurrentDivision != previous)
            {
                EventBus.Publish(new LeagueDivisionChangedEvent(CurrentDivision, CurrentDivision > previous));
            }
        }

        public void LoadState(int trophies)
        {
            Trophies = trophies;
            RecalculateDivision(false);
        }
    }
}
