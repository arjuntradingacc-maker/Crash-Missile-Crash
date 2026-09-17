using System;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Ads
{
    public readonly struct RewardedAdStartedEvent : IGameEvent { }
    public readonly struct RewardedAdCompletedEvent : IGameEvent { public readonly bool Rewarded; public RewardedAdCompletedEvent(bool rewarded) { Rewarded = rewarded; } }

    /// <summary>
    /// Provider-agnostic ad gateway. Rewarded ads are always opt-in (player-initiated, grants a
    /// bonus). Interstitials are rate-limited and must only be requested at natural transition
    /// points (level complete, returning to home) - never mid-battle.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        [SerializeField] private bool useMockProvider = true;
        private IAdProvider _provider;
        private float _lastInterstitialTime = -9999f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);

            // Swap in a real SDK-backed IAdProvider implementation here to go live with a network.
            _provider = useMockProvider ? gameObject.AddComponent<MockAdProvider>() : gameObject.AddComponent<MockAdProvider>();
            _provider.Initialize();
        }

        public bool IsRewardedReady => _provider.RewardedState == AdLoadState.Ready;

        public void ShowRewarded(Action<bool> onComplete)
        {
            EventBus.Publish(new RewardedAdStartedEvent());
            _provider.ShowRewarded(rewarded =>
            {
                EventBus.Publish(new RewardedAdCompletedEvent(rewarded));
                onComplete?.Invoke(rewarded);
            });
        }

        /// <summary>Only call this from natural transition points (post-battle, home screen), never mid-gameplay.</summary>
        public void ShowInterstitialIfAllowed(Action onComplete)
        {
            float minGap = GameDatabase.Instance != null ? GameDatabase.Instance.Economy.interstitialMinSecondsBetween : 120f;
            if (Time.unscaledTime - _lastInterstitialTime < minGap || _provider.InterstitialState != AdLoadState.Ready)
            {
                onComplete?.Invoke();
                return;
            }

            _lastInterstitialTime = Time.unscaledTime;
            _provider.ShowInterstitial(() => onComplete?.Invoke());
        }
    }
}
