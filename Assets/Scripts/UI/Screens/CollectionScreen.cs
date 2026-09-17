using System.Linq;
using CrashMissileCrash.Cannons;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    public enum CollectionTab { Units, Cannons, Champions }

    /// <summary>
    /// One tabbed roster screen serves the Collection, Cannon, Champion and Cards navigation
    /// entries from the spec - all four are different filtered views over the same card/level
    /// data, so a single reusable list avoids four near-duplicate screens.
    /// </summary>
    public class CollectionScreen : UIScreen
    {
        private RectTransform _content;
        private CollectionTab _currentTab = CollectionTab.Units;
        private Text _title;
        private Text _hintText;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "CollectionScreen", UIBuilder.PanelColor);

            _title = UIBuilder.CreateText(root, "Title", "Collection", 44, UIBuilder.TextColor);
            UIBuilder.Anchor((RectTransform)_title.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), new Vector2(0f, 80f));

            _hintText = UIBuilder.CreateText(root, "Hint", "", 26, UIBuilder.AccentColor);
            UIBuilder.Anchor((RectTransform)_hintText.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -115f), new Vector2(-40f, 40f));

            var tabRow = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var tabRect = (RectTransform)tabRow.transform;
            tabRect.SetParent(root, false);
            UIBuilder.Anchor(tabRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -160f), new Vector2(-40f, 90f));
            var hlg = tabRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            UIBuilder.CreateButton(tabRect, "TabUnits", "Units", Vector2.zero, UIBuilder.PanelColorLight, () => SelectTab(CollectionTab.Units));
            UIBuilder.CreateButton(tabRect, "TabCannons", "Cannons", Vector2.zero, UIBuilder.PanelColorLight, () => SelectTab(CollectionTab.Cannons));
            UIBuilder.CreateButton(tabRect, "TabChampions", "Champions", Vector2.zero, UIBuilder.PanelColorLight, () => SelectTab(CollectionTab.Champions));

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            var scrollRect = (RectTransform)scrollGo.transform;
            scrollRect.SetParent(root, false);
            UIBuilder.Anchor(scrollRect, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-40f, -280f));
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);
            scrollGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            _content = (RectTransform)contentGo.transform;
            _content.SetParent(scrollRect, false);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            UIBuilder.AddVerticalLayout(contentGo, 10, new RectOffset(20, 20, 10, 10));
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = _content;
            scroll.horizontal = false;
            scroll.vertical = true;

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        public void SetHint(string hint) => _hintText.text = hint;

        public void SelectTab(CollectionTab tab)
        {
            _currentTab = tab;
            RefreshList();
        }

        protected override void OnShown() => RefreshList();

        private void RefreshList()
        {
            _title.text = _currentTab switch { CollectionTab.Units => "Units", CollectionTab.Cannons => "Cannons", _ => "Champions" };

            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);

            switch (_currentTab)
            {
                case CollectionTab.Units:
                    foreach (var unit in GameDatabase.Instance.Units.Values) BuildRow(unit.id, unit.displayName, unit.rarity, CardTargetType.Unit, true);
                    break;
                case CollectionTab.Cannons:
                    foreach (var cannon in GameDatabase.Instance.Cannons.Values)
                        BuildRow(cannon.id, cannon.displayName, cannon.rarity, CardTargetType.Cannon, CannonManager.Instance != null && CannonManager.Instance.IsUnlocked(cannon.id));
                    break;
                case CollectionTab.Champions:
                    foreach (var champ in GameDatabase.Instance.Champions.Values)
                        BuildRow(champ.id, champ.displayName, champ.rarity, CardTargetType.Champion, ChampionManager.Instance != null && ChampionManager.Instance.IsUnlocked(champ.id));
                    break;
            }
        }

        private void BuildRow(string targetId, string displayName, Rarity rarity, CardTargetType type, bool unlocked)
        {
            var card = GameDatabase.Instance.Cards.Values.FirstOrDefault(c => c.targetType == type && c.targetId == targetId);
            var row = UIBuilder.CreatePanel(_content, $"Row_{targetId}", new Vector2(0f, 130f), UIBuilder.PanelColorLight);
            var layoutElement = row.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 130f;

            var icon = UIBuilder.CreatePanel(row, "Icon", new Vector2(90f, 90f), PrimitiveFactory.ColorForRarity((int)rarity));
            UIBuilder.Anchor(icon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(90f, 90f));

            int level = card != null && CardManager.Instance != null ? CardManager.Instance.GetLevel(card.id) : 1;
            string status = unlocked ? $"Lv.{level} - {rarity}" : "Locked";
            var nameText = UIBuilder.CreateText(row, "Name", $"{displayName}\n<size=22>{status}</size>", 30, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)nameText.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(80f, 0f), new Vector2(-260f, 0f));
            nameText.rectTransform.pivot = new Vector2(0f, 0.5f);

            if (card != null)
            {
                bool canUpgrade = CardManager.Instance != null && CardManager.Instance.CanUpgrade(card.id);
                var upgradeButton = UIBuilder.CreateButton(row, "Upgrade", canUpgrade ? "Upgrade" : "Locked", new Vector2(180f, 80f),
                    canUpgrade ? UIBuilder.AccentColorGreen : UIBuilder.PanelColor, () => OnUpgradeClicked(card.id, type, targetId));
                UIBuilder.Anchor(upgradeButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-110f, 0f), new Vector2(180f, 80f));
                upgradeButton.interactable = canUpgrade;
            }
        }

        private void OnUpgradeClicked(string cardId, CardTargetType type, string targetId)
        {
            if (CardManager.Instance == null || !CardManager.Instance.TryUpgrade(cardId)) return;

            if (type == CardTargetType.Cannon && CannonManager.Instance != null && !CannonManager.Instance.IsUnlocked(targetId))
                CannonManager.Instance.Unlock(targetId);
            if (type == CardTargetType.Champion && ChampionManager.Instance != null && !ChampionManager.Instance.IsUnlocked(targetId))
                ChampionManager.Instance.Unlock(targetId);

            RefreshList();
        }
    }
}
