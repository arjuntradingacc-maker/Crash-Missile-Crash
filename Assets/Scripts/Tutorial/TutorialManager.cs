using System.Collections;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Battle.Gates;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.UI;
using CrashMissileCrash.UI.Screens;
using UnityEngine;

namespace CrashMissileCrash.Tutorial
{
    public readonly struct TutorialStartedEvent : IGameEvent { }
    public readonly struct TutorialCompletedEvent : IGameEvent { }

    /// <summary>
    /// Scripts the first-run 11-step onboarding: show the cannon, teach drag-to-aim, fire units,
    /// hit a multiplication gate, watch the crowd grow, meet enemies, see automatic combat,
    /// destroy the base, collect rewards, upgrade the first card, then hand off to normal play.
    /// Every gated step also has a tap-to-continue fallback, so the tutorial can never soft-lock.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        private const string CompletedKey = "cmc_tutorial_done";
        public bool IsTutorialComplete => PlayerPrefs.GetInt(CompletedKey, 0) == 1;

        private bool _flagGateTriggered;
        private bool _flagEnemyDied;
        private bool _flagCardUpgraded;
        private bool _manualContinue;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public void RunIfNeeded()
        {
            if (IsTutorialComplete) return;
            StartCoroutine(RunTutorial());
        }

        private IEnumerator RunTutorial()
        {
            EventBus.Publish(new TutorialStartedEvent());
            EventBus.Subscribe<GateTriggeredEvent>(OnGateTriggered);
            EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Subscribe<CardUpgradedEvent>(OnCardUpgraded);

            var overlay = UIManager.Instance.GetScreen<TutorialOverlay>();
            overlay.OnContinueTapped = () => _manualContinue = true;

            GameFlowController.Instance.StartLevel("tutorial_level");
            yield return null;

            yield return Narrate(overlay, "This is your Cannon. It fires friendly units automatically!", 2.5f);
            yield return Narrate(overlay, "Drag left and right anywhere on screen to aim it.", 2.5f);
            yield return Narrate(overlay, "Your units march forward and multiply through gates.", 2.5f);
            yield return NarrateUntil(overlay, "Steer through the glowing X2 GATE up ahead!", () => _flagGateTriggered);
            yield return Narrate(overlay, "Your crowd just multiplied - more units, more power!", 2.5f);
            yield return Narrate(overlay, "Enemies ahead will fight your crowd automatically.", 2.5f);
            yield return NarrateUntil(overlay, "Watch your units engage the enemy on their own!", () => _flagEnemyDied);
            yield return NarrateUntil(overlay, "Push forward and destroy their base to win!", () => GameManager.Instance.CurrentState == GameState.Victory);

            yield return Narrate(overlay, "Victory! You earned coins, cards and XP.", 2.5f);

            var collection = UIManager.Instance.GetScreen<CollectionScreen>();
            collection?.SelectTab(CollectionTab.Units);
            UIManager.Instance.Show<CollectionScreen>(false);
            collection?.SetHint("Tap Upgrade on a card to power it up!");
            _flagCardUpgraded = false;
            while (!_flagCardUpgraded) yield return null;
            collection?.SetHint("");

            yield return Narrate(overlay, "You're ready. Let's find your first real battle!", 2f);

            PlayerPrefs.SetInt(CompletedKey, 1);
            PlayerPrefs.Save();
            EventBus.Publish(new TutorialCompletedEvent());

            EventBus.Unsubscribe<GateTriggeredEvent>(OnGateTriggered);
            EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Unsubscribe<CardUpgradedEvent>(OnCardUpgraded);

            GameFlowController.Instance.StartCurrentOrNextLevel();
        }

        private void OnGateTriggered(GateTriggeredEvent e) => _flagGateTriggered = true;
        private void OnUnitDied(UnitDiedEvent e) { if (e.Team == TeamSide.Enemy) _flagEnemyDied = true; }
        private void OnCardUpgraded(CardUpgradedEvent e) => _flagCardUpgraded = true;

        private IEnumerator Narrate(TutorialOverlay overlay, string message, float minSeconds)
        {
            UIManager.Instance.Show<TutorialOverlay>(false);
            overlay.SetMessage(message);
            _manualContinue = false;
            float t = 0f;
            while (t < minSeconds && !_manualContinue) { t += Time.unscaledDeltaTime; yield return null; }
        }

        private IEnumerator NarrateUntil(TutorialOverlay overlay, string message, System.Func<bool> predicate)
        {
            UIManager.Instance.Show<TutorialOverlay>(false);
            overlay.SetMessage(message);
            _manualContinue = false;
            while (!predicate() && !_manualContinue) yield return null;
        }
    }
}
