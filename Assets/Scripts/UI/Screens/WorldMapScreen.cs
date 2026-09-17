using System.Linq;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>World/level progression map: pick any unlocked level across all ten worlds.</summary>
    public class WorldMapScreen : UIScreen
    {
        private RectTransform _content;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "WorldMapScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "World Map", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            _content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -160f);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        protected override void OnShown()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            if (GameDatabase.Instance == null) return;

            foreach (var world in GameDatabase.Instance.Worlds.Values.OrderBy(w => w.order))
            {
                var header = UIBuilder.CreateText(_content, $"World_{world.id}", world.displayName, 32, UIBuilder.AccentColor, TextAnchor.MiddleLeft);
                header.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

                foreach (var level in GameDatabase.Instance.Levels.Values.Where(l => l.worldId == world.id).OrderBy(l => l.levelNumber))
                {
                    BuildLevelRow(level);
                }
            }
        }

        private void BuildLevelRow(LevelData level)
        {
            var row = UIBuilder.CreatePanel(_content, $"Level_{level.id}", new Vector2(0f, 100f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;

            string tag = level.isBossLevel ? " (BOSS)" : "";
            var label = UIBuilder.CreateText(row, "Label", $"Level {level.levelNumber}{tag}", 26, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-200f, 0f));

            var playButton = UIBuilder.CreateButton(row, "Play", "Play", new Vector2(160f, 70f), UIBuilder.AccentColorGreen,
                () => GameFlowController.Instance.StartLevel(level.id));
            UIBuilder.Anchor(playButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-100f, 0f), new Vector2(160f, 70f));
        }
    }
}
