using CrashMissileCrash.Battle;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>In-battle HUD: crowd count, level progress, champion ultimate button, pause.</summary>
    public class BattleHUD : UIScreen
    {
        private Text _crowdCountText;
        private Text _objectiveText;
        private RectTransform _progressFill;
        private Button _ultimateButton;
        private Image _ultimateFillImage;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "BattleHUD", new Color(0f, 0f, 0f, 0f));

            _crowdCountText = UIBuilder.CreateText(root, "CrowdCount", "0", 56, UIBuilder.TextColor);
            UIBuilder.Anchor((RectTransform)_crowdCountText.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(140f, -60f), new Vector2(260f, 90f));

            _objectiveText = UIBuilder.CreateText(root, "Objective", "Destroy the enemy base", 26, UIBuilder.TextColorDim);
            UIBuilder.Anchor((RectTransform)_objectiveText.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(700f, 50f));

            UIBuilder.CreateProgressBar(root, "LevelProgress", new Vector2(0f, 24f), UIBuilder.PanelColor, UIBuilder.AccentColorGreen, out _progressFill);
            var progressBarRect = _progressFill.parent.GetComponent<RectTransform>();
            UIBuilder.Anchor(progressBarRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -95f), new Vector2(-260f, 24f));
            _progressFill.anchorMax = new Vector2(0f, 1f);

            _ultimateButton = UIBuilder.CreateButton(root, "UltimateButton", "ULTIMATE", new Vector2(220f, 220f), UIBuilder.PanelColor, OnUltimateClicked);
            UIBuilder.Anchor(_ultimateButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 160f), new Vector2(220f, 220f));
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRect = (RectTransform)fillGo.transform;
            fillRect.SetParent(_ultimateButton.transform, false);
            UIBuilder.Stretch(fillRect);
            _ultimateFillImage = fillGo.GetComponent<Image>();
            _ultimateFillImage.color = new Color(1f, 1f, 1f, 0.15f);
            _ultimateFillImage.type = Image.Type.Filled;
            _ultimateFillImage.fillMethod = Image.FillMethod.Radial360;
            fillGo.transform.SetAsFirstSibling();

            UIBuilder.CreateButton(root, "PauseButton", "II", new Vector2(90f, 90f), UIBuilder.PanelColorLight, OnPauseClicked)
                .GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        private void OnUltimateClicked() => ChampionManager.Instance?.ActiveChampionUnit?.TryActivateUltimate();

        private void OnPauseClicked() => GameManager.Instance.PauseGame();

        private void Update()
        {
            if (!IsShown) return;

            if (CrowdManager.Instance != null) _crowdCountText.text = CrowdManager.Instance.Count.ToString("N0");

            if (BattleManager.Instance != null && BattleManager.Instance.CurrentLevel != null && BattlefieldBounds.Instance != null)
            {
                float progress = Mathf.Clamp01(CrowdManager.Instance.GetFrontZ() / Mathf.Max(1f, BattlefieldBounds.Instance.Length));
                _progressFill.anchorMax = new Vector2(progress, 1f);
            }

            var champion = ChampionManager.Instance?.ActiveChampionUnit;
            if (champion != null)
            {
                _ultimateButton.interactable = champion.IsUltimateReady;
                _ultimateFillImage.color = champion.IsUltimateReady ? new Color(1f, 0.85f, 0.1f, 0.4f) : new Color(1f, 1f, 1f, 0.1f);
            }
        }
    }
}
