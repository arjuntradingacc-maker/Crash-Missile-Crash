using CrashMissileCrash.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Season pass: free/premium tier rewards and premium pass purchase.</summary>
    public class SeasonScreen : UIScreen
    {
        private RectTransform _content;
        private Text _pointsText;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "SeasonScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "Season", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            _pointsText = UIBuilder.CreateText(root, "Points", "", 28, UIBuilder.TextColorDim);
            _pointsText.rectTransform.anchoredPosition = new Vector2(0f, 820f);

            var passButton = UIBuilder.CreateButton(root, "PremiumPass", "Unlock Premium Pass", new Vector2(500f, 90f), UIBuilder.AccentColor,
                () => { SeasonManager.Instance?.TryPurchasePremiumPass(); OnShown(); });
            passButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 750f);

            _content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -280f);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        protected override void OnShown()
        {
            var season = SeasonManager.Instance?.CurrentSeason;
            _pointsText.text = season == null ? "" : $"{season.displayName} - {SeasonManager.Instance.SeasonPoints} pts";

            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            if (season == null) return;

            foreach (var tier in season.tiers) BuildTierRow(tier);
        }

        private void BuildTierRow(Data.SeasonTierReward tier)
        {
            var row = UIBuilder.CreatePanel(_content, $"Tier_{tier.tier}", new Vector2(0f, 130f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 130f;

            bool unlocked = SeasonManager.Instance != null && SeasonManager.Instance.IsTierUnlocked(tier.tier);
            var label = UIBuilder.CreateText(row, "Label",
                $"Tier {tier.tier} {(unlocked ? "" : "(locked)")}\n<size=20>Free: +{tier.freeCoins}c  Premium: +{tier.premiumCoins}c +{tier.premiumGems}g</size>",
                24, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-260f, 0f));

            var freeButton = UIBuilder.CreateButton(row, "Free", "Free", new Vector2(110f, 80f), UIBuilder.PanelColor,
                () => { SeasonManager.Instance?.TryClaimTier(tier.tier, false); OnShown(); });
            UIBuilder.Anchor(freeButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-230f, 0f), new Vector2(110f, 80f));
            freeButton.interactable = unlocked;

            var premiumButton = UIBuilder.CreateButton(row, "Premium", "Premium", new Vector2(110f, 80f), UIBuilder.AccentColor,
                () => { SeasonManager.Instance?.TryClaimTier(tier.tier, true); OnShown(); });
            UIBuilder.Anchor(premiumButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-110f, 0f), new Vector2(110f, 80f));
            premiumButton.interactable = unlocked && SeasonManager.Instance != null && SeasonManager.Instance.HasPremiumPass;
        }
    }
}
