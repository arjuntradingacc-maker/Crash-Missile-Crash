using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrashMissileCrash.RemoteConfig
{
    /// <summary>Abstraction over a remote config service (Firebase Remote Config, a custom
    /// live-ops endpoint, ...) so balance/feature-flag tweaks can ship without an app update.</summary>
    public interface IRemoteConfigProvider
    {
        Task<Dictionary<string, string>> FetchAsync();
    }
}
