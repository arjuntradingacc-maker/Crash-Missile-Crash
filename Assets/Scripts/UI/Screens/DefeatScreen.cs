using CrashMissileCrash.Ads;
using CrashMissileCrash.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Defeat screen: Retry, Upgrade (collection), Home, and an optional rewarded-ad retry bonus.</summary>
    public class DefeatScreen : UIScreen
    {
        private Button _watchAdButton;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "DefeatScreen", new Color(0f, 0f, 0f, 0.75f));

            var panel = UIBuilder.CreatePanel(root, "Panel", new Vector2(760f, 900f), UIBuilder.PanelColor);
            UIBuilder.Anchor(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 900f));

            var title = UIBuilder.CreateText(panel, "Title", "DEFEAT", 60, UIBuilder.AccentColorRed);
            UIBuilder.Anchor((RectTransform)title.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -90f), new Vector2(0f, 100f));

            var subtitle = UIBuilder.CreateText(panel, "Subtitle", "Your army was overrun before the base fell.", 28, UIBuilder.TextColorDim);
            UIBuilder.Anchor((RectTransform)subtitle.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -220f), new Vector2(-80f, 80f));

            var retryButton = UIBuilder.CreateButton(panel, "Retry", "Retry", new Vector2(500f, 110f), UIBuilder.AccentColorGreen,
                () => GameFlowController.Instance.RetryCurrentLevel());
            UIBuilder.Anchor(retryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(500f, 110f));

            _watchAdButton = UIBuilder.CreateButton(panel, "WatchAd", "Watch Ad: Free Retry Boost", new Vector2(500f, 100f), new Color(0.35f, 0.55f, 1f),
                OnWatchAdClicked);
            UIBuilder.Anchor(_watchAdButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(500f, 100f));

            var upgradeButton = UIBuilder.CreateButton(panel, "Upgrade", "Upgrade Army", new Vector2(500f, 100f), UIBuilder.PanelColorLight,
                () => { UIManager.Instance.Show<CollectionScreen>(); });
            UIBuilder.Anchor(upgradeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(500f, 100f));

            var homeButton = UIBuilder.CreateButton(panel, "Home", "Return Home", new Vector2(500f, 100f), UIBuilder.PanelColorLight,
                () => GameFlowController.Instance.ReturnHome());
            UIBuilder.Anchor(homeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(500f, 100f));
        }

        private void OnWatchAdClicked()
        {
            if (AdManager.Instance == null) return;
            AdManager.Instance.ShowRewarded(rewarded =>
            {
                if (rewarded) GameFlowController.Instance.RetryCurrentLevel();
            });
        }

        protected override void OnShown()
        {
            _watchAdButton.interactable = AdManager.Instance != null && AdManager.Instance.IsRewardedReady;
        }
    }
}
