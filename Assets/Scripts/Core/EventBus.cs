using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Core
{
    /// <summary>
    /// Lightweight generic publish/subscribe bus used for cross-system communication
    /// so gameplay, economy, UI and live-ops systems never need direct references to each other.
    /// Any struct/class implementing IGameEvent can be published and subscribed to.
    /// </summary>
    public interface IGameEvent { }

    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!Subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                Subscribers[type] = list;
            }
            list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (Subscribers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var type = typeof(T);
            if (!Subscribers.TryGetValue(type, out var list)) return;

            // Copy to avoid mutation issues if a handler subscribes/unsubscribes during dispatch.
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ((Action<T>)list[i])?.Invoke(gameEvent);
            }
        }

        /// <summary>Clears all subscriptions. Call on full application/game restart only.</summary>
        public static void ClearAll() => Subscribers.Clear();
    }
}
