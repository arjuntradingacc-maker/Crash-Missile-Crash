using CrashMissileCrash.Backend;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using CrashMissileCrash.Progression;
using CrashMissileCrash.UI;
using CrashMissileCrash.UI.Screens;
using UnityEngine;

namespace CrashMissileCrash.Core
{
    public readonly struct LevelRewardsGrantedEvent : IGameEvent
    {
        public readonly RewardConfig Rewards;
        public LevelRewardsGrantedEvent(RewardConfig rewards) { Rewards = rewards; }
    }

    /// <summary>
    /// The glue between "battle finished" and "meta game reacts": grants rewards, advances the
    /// player's current level pointer, and drives which UI screen is shown next. Kept separate
    /// from BattleManager (which only knows about the battle itself) and from UIManager (which
    /// only knows how to show screens) so neither needs to know the other exists.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<BattleVictoryEvent>(OnVictory);
            EventBus.Subscribe<BattleDefeatEvent>(OnDefeat);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BattleVictoryEvent>(OnVictory);
            EventBus.Unsubscribe<BattleDefeatEvent>(OnDefeat);
        }

        public void StartLevel(string levelId)
        {
            if (!GameDatabase.Instance.Levels.TryGetValue(levelId, out var level))
            {
                Debug.LogWarning($"[GameFlowController] Unknown level id: {levelId}");
                return;
            }
            if (SaveManager.Instance != null) SaveManager.Instance.CurrentLevelId = levelId;

            UIManager.Instance?.GetScreen<HomeScreen>()?.Hide();
            BattleManager.Instance.StartLevel(level);
            UIManager.Instance?.Show<BattleHUD>(false);
        }

        public void StartCurrentOrNextLevel()
        {
            string levelId = SaveManager.Instance != null ? SaveManager.Instance.CurrentLevelId : "tutorial_level";
            if (!GameDatabase.Instance.Levels.ContainsKey(levelId)) levelId = "tutorial_level";
            StartLevel(levelId);
        }

        public void RetryCurrentLevel() => StartLevel(BattleManager.Instance.CurrentLevel.id);

        public void ReturnHome()
        {
            GameManager.Instance.ChangeState(GameState.Home);
            UIManager.Instance?.Show<HomeScreen>(false);
        }

        private void OnVictory(BattleVictoryEvent e)
        {
            var level = BattleManager.Instance.CurrentLevel;
            GrantRewards(level.rewards);

            var next = GameDatabase.Instance.GetNextLevel(level.id);
            if (next != null && SaveManager.Instance != null) SaveManager.Instance.CurrentLevelId = next.id;

            UIManager.Instance?.Show<VictoryScreen>(false);
        }

        private void OnDefeat(BattleDefeatEvent e)
        {
            UIManager.Instance?.Show<DefeatScreen>(false);
        }

        private void GrantRewards(RewardConfig rewards)
        {
            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                economy.Add(CurrencyType.Coins, rewards.coins);
                economy.Add(CurrencyType.Gems, rewards.gems);
            }
            if (ServiceLocator.TryGet<ProgressionManager>(out var progression))
            {
                progression.AddXp(rewards.xp);
            }
            if (!string.IsNullOrEmpty(rewards.guaranteedCardId) && ServiceLocator.TryGet<CardManager>(out var cards))
            {
                cards.AddCards(rewards.guaranteedCardId, rewards.guaranteedCardCount);
            }
            if (ServiceLocator.TryGet<SeasonManager>(out var season))
            {
                season.AddPoints(Mathf.Max(5, rewards.stars * 5));
            }
            EventBus.Publish(new LevelRewardsGrantedEvent(rewards));
        }
    }
}
