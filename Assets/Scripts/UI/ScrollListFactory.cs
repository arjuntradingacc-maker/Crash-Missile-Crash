using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI
{
    /// <summary>Builds a vertically-scrolling list (ScrollRect + masked viewport + auto-sizing
    /// content with a VerticalLayoutGroup) so every list screen shares one implementation.</summary>
    public static class ScrollListFactory
    {
        public static RectTransform Build(Transform parent, out RectTransform viewport)
        {
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            viewport = (RectTransform)scrollGo.transform;
            viewport.SetParent(parent, false);
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);
            scrollGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            var content = (RectTransform)contentGo.transform;
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            UIBuilder.AddVerticalLayout(contentGo, 10, new RectOffset(10, 10, 10, 10));
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;

            return content;
        }
    }
}
