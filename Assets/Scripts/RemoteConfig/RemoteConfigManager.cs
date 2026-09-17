using System.Collections.Generic;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.RemoteConfig
{
    public readonly struct RemoteConfigLoadedEvent : IGameEvent { }

    /// <summary>Typed accessors over a flat string key/value remote config table.</summary>
    public class RemoteConfigManager : MonoBehaviour
    {
        public static RemoteConfigManager Instance { get; private set; }

        private IRemoteConfigProvider _provider;
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            _provider = new LocalRemoteConfigProvider();
        }

        private async void Start()
        {
            _values = await _provider.FetchAsync();
            EventBus.Publish(new RemoteConfigLoadedEvent());
        }

        public string GetString(string key, string fallback = "") => _values.TryGetValue(key, out var v) ? v : fallback;

        public int GetInt(string key, int fallback = 0) => _values.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : fallback;

        public float GetFloat(string key, float fallback = 0f) => _values.TryGetValue(key, out var v) && float.TryParse(v, out var f) ? f : fallback;

        public bool GetBool(string key, bool fallback = false) => _values.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : fallback;
    }
}
