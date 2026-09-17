using System.Collections.Generic;

namespace CrashMissileCrash.Analytics
{
    /// <summary>Abstraction over a third-party analytics SDK (Firebase, Amplitude, GameAnalytics, ...).</summary>
    public interface IAnalyticsProvider
    {
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters);
        void SetUserProperty(string key, string value);
    }
}
