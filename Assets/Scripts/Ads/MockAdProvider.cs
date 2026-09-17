using System;
using System.Collections;
using UnityEngine;

namespace CrashMissileCrash.Ads
{
    /// <summary>Simulates ad network latency/fill without any third-party SDK, so the full ad
    /// flow (load -> ready -> show -> reward) is testable before a real network is integrated.</summary>
    public class MockAdProvider : MonoBehaviour, IAdProvider
    {
        public AdLoadState RewardedState { get; private set; } = AdLoadState.NotLoaded;
        public AdLoadState InterstitialState { get; private set; } = AdLoadState.NotLoaded;

        public void Initialize()
        {
            LoadRewarded();
            LoadInterstitial();
        }

        public void LoadRewarded()
        {
            if (RewardedState == AdLoadState.Loading) return;
            RewardedState = AdLoadState.Loading;
            StartCoroutine(SimulateLoad(1.5f, () => RewardedState = AdLoadState.Ready));
        }

        public void ShowRewarded(Action<bool> onComplete)
        {
            if (RewardedState != AdLoadState.Ready)
            {
                onComplete?.Invoke(false);
                return;
            }
            RewardedState = AdLoadState.NotLoaded;
            StartCoroutine(SimulatePlayback(() =>
            {
                onComplete?.Invoke(true);
                LoadRewarded();
            }));
        }

        public void LoadInterstitial()
        {
            if (InterstitialState == AdLoadState.Loading) return;
            InterstitialState = AdLoadState.Loading;
            StartCoroutine(SimulateLoad(1f, () => InterstitialState = AdLoadState.Ready));
        }

        public void ShowInterstitial(Action onComplete)
        {
            if (InterstitialState != AdLoadState.Ready)
            {
                onComplete?.Invoke();
                return;
            }
            InterstitialState = AdLoadState.NotLoaded;
            StartCoroutine(SimulatePlayback(() =>
            {
                onComplete?.Invoke();
                LoadInterstitial();
            }));
        }

        private IEnumerator SimulateLoad(float delay, Action onLoaded)
        {
            yield return new WaitForSeconds(delay);
            onLoaded?.Invoke();
        }

        private IEnumerator SimulatePlayback(Action onFinished)
        {
            yield return new WaitForSeconds(0.5f);
            onFinished?.Invoke();
        }
    }
}
