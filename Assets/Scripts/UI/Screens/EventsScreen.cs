using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Events;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Active limited-time events (Boss Rush, Time Attack, Treasure Hunt, ...) with score/rewards.</summary>
    public class EventsScreen : UIScreen
    {
        private RectTransform _content;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "EventsScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "Events", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            _content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -160f);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        protected override void OnShown()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            if (EventManager.Instance == null) return;

            bool any = false;
            foreach (var evt in EventManager.Instance.GetActiveEvents())
            {
                any = true;
                BuildRow(evt);
            }
            if (!any)
            {
                var empty = UIBuilder.CreateText(_content, "Empty", "No active events right now. Check back soon!", 28, UIBuilder.TextColorDim);
                empty.gameObject.AddComponent<LayoutElement>().preferredHeight = 200f;
            }
        }

        private void BuildRow(LiveEventData evt)
        {
            var row = UIBuilder.CreatePanel(_content, $"Event_{evt.id}", new Vector2(0f, 140f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 140f;

            int score = EventManager.Instance.GetScore(evt.id);
            var label = UIBuilder.CreateText(row, "Label", $"{evt.displayName}\n<size=20>{evt.format} - Score: {score}</size>", 28, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-230f, 0f));

            var playButton = UIBuilder.CreateButton(row, "Play", "Play", new Vector2(200f, 80f), UIBuilder.AccentColorGreen, () =>
            {
                if (evt.levelIds.Count > 0) GameFlowController.Instance.StartLevel(evt.levelIds[0]);
            });
            UIBuilder.Anchor(playButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(200f, 80f));
        }
    }
}
