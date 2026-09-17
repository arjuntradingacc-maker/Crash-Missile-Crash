using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrashMissileCrash.UI
{
    /// <summary>
    /// Procedurally constructs consistent-looking uGUI widgets so no hand-authored prefabs are
    /// needed for any screen. Centralizing style here (colors, fonts, padding) means the whole
    /// game's UI can be re-themed by editing one file.
    /// </summary>
    public static class UIBuilder
    {
        public static readonly Color PanelColor = new Color(0.10f, 0.11f, 0.16f, 0.94f);
        public static readonly Color PanelColorLight = new Color(0.16f, 0.18f, 0.26f, 0.96f);
        public static readonly Color AccentColor = new Color(1.0f, 0.72f, 0.15f);
        public static readonly Color AccentColorGreen = new Color(0.30f, 0.85f, 0.45f);
        public static readonly Color AccentColorRed = new Color(0.92f, 0.30f, 0.30f);
        public static readonly Color TextColor = new Color(0.96f, 0.96f, 0.98f);
        public static readonly Color TextColorDim = new Color(0.72f, 0.74f, 0.8f);

        public static RectTransform CreateFullScreenPanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            go.GetComponent<Image>().color = color;
            return rect;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = anchor;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = color;

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = color * 1.15f;
            colors.pressedColor = color * 0.8f;
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);
            go.AddComponent<ButtonPunch>();

            if (!string.IsNullOrEmpty(label))
            {
                CreateText(rect, "Label", label, Mathf.RoundToInt(size.y * 0.42f), TextColor);
            }
            return button;
        }

        public static Image CreateProgressBar(Transform parent, string name, Vector2 size, Color backColor, Color fillColor, out RectTransform fillRect)
        {
            var back = CreatePanel(parent, name, size, backColor);
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(back, false);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillGo.GetComponent<Image>().color = fillColor;
            return back.GetComponent<Image>();
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
        }

        public static VerticalLayoutGroup AddVerticalLayout(GameObject go, int spacing = 8, RectOffset padding = null)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(12, 12, 12, 12);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static GridLayoutGroup AddGridLayout(GameObject go, Vector2 cellSize, int spacing = 10)
        {
            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = new Vector2(spacing, spacing);
            grid.padding = new RectOffset(12, 12, 12, 12);
            return grid;
        }
    }

    /// <summary>Small press/release scale animation applied to every button for tactile feedback.</summary>
    public class ButtonPunch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Vector3 _baseScale;
        private void Awake() => _baseScale = transform.localScale;
        public void OnPointerDown(PointerEventData eventData) => transform.localScale = _baseScale * 0.92f;
        public void OnPointerUp(PointerEventData eventData) => transform.localScale = _baseScale;
    }
}
