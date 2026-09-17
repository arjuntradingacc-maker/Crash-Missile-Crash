using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CrashMissileCrash.RemoteConfig
{
    [System.Serializable] public class RemoteConfigEntry { public string key; public string value; }
    [System.Serializable] public class RemoteConfigDefaults { public List<RemoteConfigEntry> entries = new List<RemoteConfigEntry>(); }

    /// <summary>Reads defaults shipped in the build (Resources/Data/remote_config_defaults.json).
    /// A real provider would fetch from a live-ops server and fall back to these same defaults
    /// on network failure, so offline play is never blocked on remote config.</summary>
    public class LocalRemoteConfigProvider : IRemoteConfigProvider
    {
        public Task<Dictionary<string, string>> FetchAsync()
        {
            var result = new Dictionary<string, string>();
            var textAsset = Resources.Load<TextAsset>("Data/remote_config_defaults");
            if (textAsset != null)
            {
                var defaults = JsonUtility.FromJson<RemoteConfigDefaults>(textAsset.text);
                foreach (var entry in defaults.entries) result[entry.key] = entry.value;
            }
            return Task.FromResult(result);
        }
    }
}
