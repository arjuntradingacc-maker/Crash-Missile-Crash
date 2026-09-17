using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Core
{
    /// <summary>
    /// Minimal service locator used to wire manager singletons without static-class coupling.
    /// Managers register themselves on Awake/Init and are resolved by interface where possible,
    /// so implementations (e.g. mock vs. real backend/ads/IAP) can be swapped without touching callers.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            Services[typeof(T)] = service;
        }

        public static void Unregister<T>()
        {
            Services.Remove(typeof(T));
        }

        public static T Get<T>()
        {
            if (Services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            UnityEngine.Debug.LogWarning($"[ServiceLocator] No service registered for {typeof(T).Name}");
            return default;
        }

        public static bool TryGet<T>(out T service)
        {
            if (Services.TryGetValue(typeof(T), out var raw))
            {
                service = (T)raw;
                return true;
            }
            service = default;
            return false;
        }

        public static bool IsRegistered<T>() => Services.ContainsKey(typeof(T));

        /// <summary>Call only when tearing down the whole application/game session (e.g. for tests).</summary>
        public static void Clear() => Services.Clear();
    }
}
