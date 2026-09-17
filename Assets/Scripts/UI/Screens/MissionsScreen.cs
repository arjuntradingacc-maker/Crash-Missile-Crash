using CrashMissileCrash.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Lists daily/weekly/achievement/milestone missions with claimable rewards.</summary>
    public class MissionsScreen : UIScreen
    {
        private RectTransform _content;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "MissionsScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "Missions", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            _content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -160f);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        protected override void OnShown()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            if (Data.GameDatabase.Instance == null) return;

            foreach (var mission in Data.GameDatabase.Instance.Missions.Values) BuildRow(mission);
        }

        private void BuildRow(Data.MissionData mission)
        {
            var row = UIBuilder.CreatePanel(_content, $"Mission_{mission.id}", new Vector2(0f, 130f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 130f;

            int progress = MissionManager.Instance != null ? MissionManager.Instance.GetProgress(mission.id) : 0;
            bool claimed = MissionManager.Instance != null && MissionManager.Instance.IsClaimed(mission.id);
            bool complete = MissionManager.Instance != null && MissionManager.Instance.IsComplete(mission.id);

            var label = UIBuilder.CreateText(row, "Label",
                $"{mission.cadence}: {mission.metric} ({progress}/{mission.targetValue})\n<size=20>+{mission.rewardCoins} coins, +{mission.rewardGems} gems</size>",
                26, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-230f, 0f));

            string btnLabel = claimed ? "Claimed" : complete ? "Claim" : "In Progress";
            var claimButton = UIBuilder.CreateButton(row, "Claim", btnLabel, new Vector2(200f, 80f),
                claimed ? UIBuilder.PanelColor : complete ? UIBuilder.AccentColorGreen : UIBuilder.PanelColor,
                () => { MissionManager.Instance?.TryClaim(mission.id); OnShown(); });
            claimButton.interactable = complete && !claimed;
            UIBuilder.Anchor(claimButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(200f, 80f));
        }
    }
}
