using CrashMissileCrash.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Music/SFX volume, haptics, reduced-effects accessibility toggle, graphics quality.</summary>
    public class SettingsScreen : UIScreen
    {
        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "SettingsScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "Settings", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            var content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -160f);

            BuildSlider(content, "Music Volume", AudioSettingsStore.MusicVolume, v => { AudioSettingsStore.MusicVolume = v; AudioManager.Instance?.ApplySettings(); });
            BuildSlider(content, "SFX Volume", AudioSettingsStore.SfxVolume, v => { AudioSettingsStore.SfxVolume = v; AudioManager.Instance?.ApplySettings(); });
            BuildToggleRow(content, "Haptics", AudioSettingsStore.HapticsEnabled, v => AudioSettingsStore.HapticsEnabled = v);
            BuildToggleRow(content, "Reduced Effects (Accessibility)", AudioSettingsStore.ReducedEffects, v => AudioSettingsStore.ReducedEffects = v);

            BuildQualityRow(content);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        private void BuildSlider(Transform parent, string label, float initial, System.Action<float> onChanged)
        {
            var row = UIBuilder.CreatePanel(parent, $"Slider_{label}", new Vector2(0f, 110f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;
            var text = UIBuilder.CreateText(row, "Label", label, 26, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)text.transform, new Vector2(0f, 0.5f), new Vector2(0.4f, 1f), new Vector2(30f, 0f), Vector2.zero);

            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var sliderRect = (RectTransform)sliderGo.transform;
            sliderRect.SetParent(row, false);
            UIBuilder.Anchor(sliderRect, new Vector2(0.45f, 0.5f), new Vector2(0.95f, 0.5f), Vector2.zero, new Vector2(0f, 20f));
            var slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = initial;
            slider.onValueChanged.AddListener(v => onChanged(v));

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(sliderRect, false);
            UIBuilder.Stretch((RectTransform)bgGo.transform);
            bgGo.GetComponent<Image>().color = UIBuilder.PanelColor;
            slider.targetGraphic = bgGo.GetComponent<Image>();
        }

        private void BuildToggleRow(Transform parent, string label, bool initial, System.Action<bool> onChanged)
        {
            var row = UIBuilder.CreatePanel(parent, $"Toggle_{label}", new Vector2(0f, 100f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;
            var text = UIBuilder.CreateText(row, "Label", label, 26, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)text.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-140f, 0f));

            var toggleGo = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            var toggleRect = (RectTransform)toggleGo.transform;
            toggleRect.SetParent(row, false);
            UIBuilder.Anchor(toggleRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-70f, 0f), new Vector2(70f, 70f));
            toggleGo.GetComponent<Image>().color = UIBuilder.PanelColor;
            var toggle = toggleGo.GetComponent<Toggle>();
            toggle.isOn = initial;

            var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkGo.transform.SetParent(toggleRect, false);
            UIBuilder.Stretch((RectTransform)checkGo.transform);
            checkGo.GetComponent<Image>().color = UIBuilder.AccentColorGreen;
            toggle.graphic = checkGo.GetComponent<Image>();
            toggle.onValueChanged.AddListener(v => onChanged(v));
        }

        private void BuildQualityRow(Transform parent)
        {
            var row = new GameObject("QualityRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.GetComponent<RectTransform>().SetParent(parent, false);
            row.AddComponent<LayoutElement>().preferredHeight = 90f;
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            string[] labels = { "Low", "Medium", "High", "Ultra" };
            for (int i = 0; i < labels.Length; i++)
            {
                int level = i;
                UIBuilder.CreateButton(row.transform, $"Quality_{labels[i]}", labels[i], Vector2.zero, UIBuilder.PanelColorLight,
                    () => QualitySettings.SetQualityLevel(level, true));
            }
        }
    }

    /// <summary>Simple persisted settings store backing this screen's controls.</summary>
    public static class AudioSettingsStore
    {
        public static float MusicVolume { get => PlayerPrefs.GetFloat("cmc_music_vol", 0.8f); set => PlayerPrefs.SetFloat("cmc_music_vol", value); }
        public static float SfxVolume { get => PlayerPrefs.GetFloat("cmc_sfx_vol", 1f); set => PlayerPrefs.SetFloat("cmc_sfx_vol", value); }
        public static bool HapticsEnabled { get => PlayerPrefs.GetInt("cmc_haptics", 1) == 1; set => PlayerPrefs.SetInt("cmc_haptics", value ? 1 : 0); }
        public static bool ReducedEffects { get => PlayerPrefs.GetInt("cmc_reduced_fx", 0) == 1; set => PlayerPrefs.SetInt("cmc_reduced_fx", value ? 1 : 0); }
    }
}
