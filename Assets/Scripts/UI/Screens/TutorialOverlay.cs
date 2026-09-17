using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>A small message bubble with a "Got it" continue affordance, used to narrate the
    /// tutorial. Always tap-to-advance as a fallback even when a step also auto-advances on a
    /// gameplay event, so the tutorial can never get stuck waiting on player behavior.</summary>
    public class TutorialOverlay : UIScreen
    {
        private Text _messageText;
        public System.Action OnContinueTapped;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "TutorialOverlay", new Color(0f, 0f, 0f, 0f));

            var bubble = UIBuilder.CreatePanel(root, "Bubble", new Vector2(900f, 220f), UIBuilder.PanelColor);
            UIBuilder.Anchor(bubble, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(900f, 220f));

            _messageText = UIBuilder.CreateText(bubble, "Message", "", 32, UIBuilder.TextColor);
            UIBuilder.Anchor((RectTransform)_messageText.transform, Vector2.zero, Vector2.one, new Vector2(0f, 40f), new Vector2(-40f, -20f));

            var continueButton = UIBuilder.CreateButton(bubble, "Continue", "Got it", new Vector2(240f, 70f), UIBuilder.AccentColorGreen,
                () => OnContinueTapped?.Invoke());
            UIBuilder.Anchor(continueButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(240f, 70f));
        }

        public void SetMessage(string message) => _messageText.text = message;
    }
}
