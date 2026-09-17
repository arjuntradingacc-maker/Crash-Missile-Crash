using System;

namespace CrashMissileCrash.Ads
{
    public enum AdLoadState { NotLoaded, Loading, Ready, Failed }

    /// <summary>Abstraction over a third-party ad SDK (AdMob, Unity Ads, IronSource, ...).
    /// Swap the concrete provider in AdManager.Awake to change networks without touching callers.</summary>
    public interface IAdProvider
    {
        void Initialize();
        AdLoadState RewardedState { get; }
        AdLoadState InterstitialState { get; }

        void LoadRewarded();
        void ShowRewarded(Action<bool> onComplete); // bool = whether the reward should be granted

        void LoadInterstitial();
        void ShowInterstitial(Action onComplete);
    }
}
