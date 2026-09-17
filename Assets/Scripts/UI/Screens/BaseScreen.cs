using System;
using CrashMissileCrash.Base;
using CrashMissileCrash.Raids;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Home base building/upgrades, shield activation, and entry point into raids.</summary>
    public class BaseScreen : UIScreen
    {
        private RectTransform _content;
        private Text _shieldText;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "BaseScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "Base", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            _shieldText = UIBuilder.CreateText(root, "Shield", "", 26, UIBuilder.TextColorDim);
            _shieldText.rectTransform.anchoredPosition = new Vector2(-200f, 820f);

            var shieldButton = UIBuilder.CreateButton(root, "ActivateShield", "Activate Shield (8h)", new Vector2(360f, 80f), new Color(0.3f, 0.6f, 1f),
                () => { BaseManager.Instance?.ActivateShield(TimeSpan.FromHours(8)); RefreshShield(); });
            shieldButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, 820f);

            var raidButton = UIBuilder.CreateButton(root, "RaidButton", "Raid Opponents", new Vector2(500f, 100f), UIBuilder.AccentColorGreen, OnRaidClicked);
            raidButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 740f);

            _content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -350f);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        protected override void OnShown()
        {
            RefreshShield();
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            if (BaseManager.Instance == null) return;

            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType))) BuildRow(type);
        }

        private void RefreshShield()
        {
            if (BaseManager.Instance == null) return;
            _shieldText.text = BaseManager.Instance.IsShieldActive ? "Shield: ACTIVE" : "Shield: inactive";
        }

        private void BuildRow(BuildingType type)
        {
            var row = UIBuilder.CreatePanel(_content, $"Building_{type}", new Vector2(0f, 120f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 120f;

            int level = BaseManager.Instance.GetLevel(type);
            var label = UIBuilder.CreateText(row, "Label", $"{type}\n<size=20>Level {level}</size>", 28, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-230f, 0f));

            var upgradeButton = UIBuilder.CreateButton(row, "Upgrade",
                $"{BaseManager.Instance.UpgradeCoinCost(type)}c", new Vector2(200f, 80f), UIBuilder.AccentColorGreen,
                () => { BaseManager.Instance?.TryUpgrade(type); OnShown(); });
            UIBuilder.Anchor(upgradeButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(200f, 80f));
        }

        private async void OnRaidClicked()
        {
            if (RaidManager.Instance == null) return;
            var opponent = await RaidManager.Instance.FindOpponentAsync();
            if (opponent == null) return;
            await RaidManager.Instance.AttackAsync(opponent);
            OnShown();
        }
    }
}
