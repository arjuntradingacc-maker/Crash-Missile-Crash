using System;
using System.Collections.Generic;
using CrashMissileCrash.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrashMissileCrash.UI
{
    /// <summary>Owns the root Canvas and a simple single-active-screen stack. Individual screens
    /// register themselves at creation time; navigation is just "hide current, show requested".</summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public Canvas RootCanvas { get; private set; }
        public RectTransform RootRect { get; private set; }

        private readonly Dictionary<Type, UIScreen> _screens = new Dictionary<Type, UIScreen>();
        private readonly Stack<Type> _history = new Stack<Type>();
        private UIScreen _currentScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            BuildCanvas();
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            RootCanvas = canvasGo.GetComponent<Canvas>();
            RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            RootCanvas.sortingOrder = 10;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            RootRect = canvasGo.GetComponent<RectTransform>();

            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                esGo.transform.SetParent(transform, false);
            }
        }

        public void RegisterScreen<T>(T screen) where T : UIScreen
        {
            _screens[typeof(T)] = screen;
            screen.gameObject.SetActive(false);
        }

        public T GetScreen<T>() where T : UIScreen => _screens.TryGetValue(typeof(T), out var s) ? (T)s : null;

        public void Show<T>(bool addToHistory = true) where T : UIScreen
        {
            if (!_screens.TryGetValue(typeof(T), out var screen))
            {
                Debug.LogWarning($"[UIManager] Screen not registered: {typeof(T).Name}");
                return;
            }

            if (_currentScreen != null && _currentScreen != screen)
            {
                _currentScreen.Hide();
                if (addToHistory) _history.Push(GetTypeOf(_currentScreen));
            }

            _currentScreen = screen;
            screen.Show();
        }

        public void GoBack()
        {
            if (_history.Count == 0) return;
            var previousType = _history.Pop();
            if (_screens.TryGetValue(previousType, out var screen))
            {
                _currentScreen?.Hide();
                _currentScreen = screen;
                screen.Show();
            }
        }

        private static Type GetTypeOf(UIScreen screen) => screen.GetType();
    }
}
