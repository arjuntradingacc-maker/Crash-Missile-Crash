using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>The command-center home screen: resource bar, big Play button, and navigation
    /// to every other meta screen.</summary>
    public class HomeScreen : UIScreen
    {
        private Text _coinsText;
        private Text _gemsText;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "HomeScreen", new Color(0f, 0f, 0f, 0f));

            BuildResourceBar(root);
            BuildPlayButton(root);
            BuildNavGrid(root);
        }

        private void BuildResourceBar(Transform root)
        {
            var bar = UIBuilder.CreatePanel(root, "ResourceBar", new Vector2(0f, 110f), UIBuilder.PanelColor);
            var barRect = (RectTransform)bar;
            UIBuilder.Anchor(barRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -20f), new Vector2(-20f, 110f));

            _coinsText = UIBuilder.CreateText(bar, "Coins", "0", 34, UIBuilder.AccentColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)_coinsText.transform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(30f, 0f), new Vector2(0f, 0f));

            _gemsText = UIBuilder.CreateText(bar, "Gems", "0", 34, new Color(0.4f, 0.85f, 1f), TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)_gemsText.transform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-30f, 0f));
        }

        private void BuildPlayButton(Transform root)
        {
            var button = UIBuilder.CreateButton(root, "PlayButton", "BATTLE", new Vector2(500f, 160f), UIBuilder.AccentColorGreen,
                () => GameFlowController.Instance.StartCurrentOrNextLevel());
            UIBuilder.Anchor(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(500f, 160f));
        }

        private void BuildNavGrid(Transform root)
        {
            var gridGo = new GameObject("NavGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            var gridRect = (RectTransform)gridGo.transform;
            gridRect.SetParent(root, false);
            UIBuilder.Anchor(gridRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 140f), new Vector2(-20f, 420f));
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(240f, 190f);
            grid.spacing = new Vector2(10f, 10f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            AddNavButton(gridRect, "Collection", UIBuilder.PanelColorLight, () => { UIManager.Instance.GetScreen<CollectionScreen>()?.SelectTab(CollectionTab.Units); UIManager.Instance.Show<CollectionScreen>(); });
            AddNavButton(gridRect, "Cannon", UIBuilder.PanelColorLight, () => { UIManager.Instance.GetScreen<CollectionScreen>()?.SelectTab(CollectionTab.Cannons); UIManager.Instance.Show<CollectionScreen>(); });
            AddNavButton(gridRect, "Champion", UIBuilder.PanelColorLight, () => { UIManager.Instance.GetScreen<CollectionScreen>()?.SelectTab(CollectionTab.Champions); UIManager.Instance.Show<CollectionScreen>(); });
            AddNavButton(gridRect, "Cards", UIBuilder.PanelColorLight, () => { UIManager.Instance.GetScreen<CollectionScreen>()?.SelectTab(CollectionTab.Units); UIManager.Instance.Show<CollectionScreen>(); });
            AddNavButton(gridRect, "Base", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<BaseScreen>());
            AddNavButton(gridRect, "Events", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<EventsScreen>());
            AddNavButton(gridRect, "Missions", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<MissionsScreen>());
            AddNavButton(gridRect, "Season", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<SeasonScreen>());
            AddNavButton(gridRect, "Shop", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<ShopScreen>());
            AddNavButton(gridRect, "World Map", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<WorldMapScreen>());
            AddNavButton(gridRect, "Settings", UIBuilder.PanelColorLight, () => UIManager.Instance.Show<SettingsScreen>());
        }

        private void AddNavButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            UIBuilder.CreateButton(parent, $"Nav_{label}", label, Vector2.zero, color, onClick);
        }

        private void Update()
        {
            if (!IsShown || !ServiceLocator.IsRegistered<EconomyManager>()) return;
            var economy = ServiceLocator.Get<EconomyManager>();
            _coinsText.text = $"Coins: {economy.GetBalance(CurrencyType.Coins):N0}";
            _gemsText.text = $"Gems: {economy.GetBalance(CurrencyType.Gems):N0}";
        }
    }
}
