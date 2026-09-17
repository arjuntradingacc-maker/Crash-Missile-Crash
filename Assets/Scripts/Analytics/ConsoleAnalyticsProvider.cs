using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrashMissileCrash.Analytics
{
    /// <summary>Logs to the console/Editor log. Swap for a real SDK-backed provider in production.</summary>
    public class ConsoleAnalyticsProvider : IAnalyticsProvider
    {
        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            string paramString = parameters == null ? "" : string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"));
            Debug.Log($"[Analytics] {eventName} ({paramString})");
        }

        public void SetUserProperty(string key, string value)
        {
            Debug.Log($"[Analytics] UserProperty {key}={value}");
        }
    }
}
