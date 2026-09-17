using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Post-battle victory summary: survivors, coins/cards/xp/stars, continue.</summary>
    public class VictoryScreen : UIScreen
    {
        private Text _summaryText;
        private Text _titleText;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "VictoryScreen", new Color(0f, 0f, 0f, 0.7f));

            var panel = UIBuilder.CreatePanel(root, "Panel", new Vector2(760f, 900f), UIBuilder.PanelColor);
            UIBuilder.Anchor(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 900f));

            _titleText = UIBuilder.CreateText(panel, "Title", "VICTORY!", 64, UIBuilder.AccentColorGreen);
            UIBuilder.Anchor((RectTransform)_titleText.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -90f), new Vector2(0f, 100f));

            _summaryText = UIBuilder.CreateText(panel, "Summary", "", 32, UIBuilder.TextColor);
            UIBuilder.Anchor((RectTransform)_summaryText.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(600f, 400f));

            var continueButton = UIBuilder.CreateButton(panel, "Continue", "Continue", new Vector2(420f, 110f), UIBuilder.AccentColorGreen,
                () => GameFlowController.Instance.StartCurrentOrNextLevel());
            UIBuilder.Anchor(continueButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(420f, 110f));
        }

        protected override void OnShown()
        {
            var level = BattleManager.Instance.CurrentLevel;
            var rewards = level.rewards;
            int survivors = CrowdManager.Instance != null ? CrowdManager.Instance.Count : 0;

            _summaryText.text = $"Surviving units: {survivors}\n\n" +
                                 $"Coins: +{rewards.coins}\nGems: +{rewards.gems}\nXP: +{rewards.xp}\n" +
                                 (string.IsNullOrEmpty(rewards.guaranteedCardId) ? "" : $"Cards: +{rewards.guaranteedCardCount} {rewards.guaranteedCardId}\n") +
                                 $"\nStars: {new string('*', Mathf.Clamp(rewards.stars, 0, 3))}";
        }
    }
}
