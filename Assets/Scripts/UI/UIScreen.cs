using System.Collections;
using UnityEngine;

namespace CrashMissileCrash.UI
{
    /// <summary>Base class for a full-screen UI panel managed by UIManager's screen stack.</summary>
    public abstract class UIScreen : MonoBehaviour
    {
        protected CanvasGroup CanvasGroup;
        public bool IsShown { get; private set; }

        protected virtual void Awake()
        {
            CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (CanvasGroup == null) CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            IsShown = true;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f, 0.2f));
            OnShown();
        }

        public virtual void Hide()
        {
            IsShown = false;
            OnHidden();
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDisable());
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }

        private IEnumerator FadeOutAndDisable()
        {
            yield return FadeTo(0f, 0.15f);
            gameObject.SetActive(false);
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float start = CanvasGroup.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                CanvasGroup.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            CanvasGroup.alpha = target;
        }
    }
}
