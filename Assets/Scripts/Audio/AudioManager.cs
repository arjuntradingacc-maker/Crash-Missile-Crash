using System.Collections.Generic;
using CrashMissileCrash.Ads;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Battle.Gates;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using CrashMissileCrash.UI.Screens;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Audio
{
    /// <summary>
    /// Central audio hub: procedurally-synthesized SFX for every gameplay event (no authored
    /// audio assets required to be fully playable), plus a music AudioSource with a two-layer
    /// crossfade for dynamic intensity - assign real composed clips (Normal/Intense/Boss/Event)
    /// in the inspector to replace silence with real music without touching any gameplay code.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music (optional - assign real composed clips here)")]
        public AudioClip NormalMusicClip;
        public AudioClip IntenseMusicClip;
        public AudioClip BossMusicClip;
        public AudioClip EventMusicClip;

        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>();
        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
        private int _sfxCursor;
        private const int SfxPoolSize = 12;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);

            _musicSourceA = gameObject.AddComponent<AudioSource>();
            _musicSourceB = gameObject.AddComponent<AudioSource>();
            foreach (var src in new[] { _musicSourceA, _musicSourceB }) { src.loop = true; src.playOnAwake = false; src.volume = 0f; }

            for (int i = 0; i < SfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool.Add(src);
            }

            ApplySettings();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CannonShotFiredEvent>(OnCannonShot);
            EventBus.Subscribe<GateVisualPulseEvent>(OnGatePulse);
            EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Subscribe<BaseCoreDestroyedEvent>(OnBaseDestroyed);
            EventBus.Subscribe<CardUpgradedEvent>(OnCardUpgraded);
            EventBus.Subscribe<PackOpenedEvent>(OnPackOpened);
            EventBus.Subscribe<ChampionUltimateFiredEvent>(OnUltimateFired);
            EventBus.Subscribe<BattleVictoryEvent>(OnVictory);
            EventBus.Subscribe<BattleDefeatEvent>(OnDefeat);
            EventBus.Subscribe<RewardedAdCompletedEvent>(OnRewardedAdCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CannonShotFiredEvent>(OnCannonShot);
            EventBus.Unsubscribe<GateVisualPulseEvent>(OnGatePulse);
            EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Unsubscribe<BaseCoreDestroyedEvent>(OnBaseDestroyed);
            EventBus.Unsubscribe<CardUpgradedEvent>(OnCardUpgraded);
            EventBus.Unsubscribe<PackOpenedEvent>(OnPackOpened);
            EventBus.Unsubscribe<ChampionUltimateFiredEvent>(OnUltimateFired);
            EventBus.Unsubscribe<BattleVictoryEvent>(OnVictory);
            EventBus.Unsubscribe<BattleDefeatEvent>(OnDefeat);
            EventBus.Unsubscribe<RewardedAdCompletedEvent>(OnRewardedAdCompleted);
        }

        private void OnCannonShot(CannonShotFiredEvent e) => Play("cannon_fire", 800f, 0.08f, ToneShape.Square, 0.25f);
        private void OnGatePulse(GateVisualPulseEvent e) => Play("gate", 500f, 0.25f, ToneShape.Triangle, 0.5f);
        private void OnBaseDestroyed(BaseCoreDestroyedEvent e) => Play("base_destroy", 90f, 0.9f, ToneShape.Noise, 0.8f);
        private void OnCardUpgraded(CardUpgradedEvent e) => Play("upgrade", 660f, 0.3f, ToneShape.Sine, 0.5f);
        private void OnPackOpened(PackOpenedEvent e) => Play("pack_open", 440f, 0.4f, ToneShape.Sine, 0.5f);
        private void OnUltimateFired(ChampionUltimateFiredEvent e) => Play("ultimate", 220f, 0.5f, ToneShape.Square, 0.6f);
        private void OnVictory(BattleVictoryEvent e) => Play("victory", 523f, 0.6f, ToneShape.Triangle, 0.6f);
        private void OnDefeat(BattleDefeatEvent e) => Play("defeat", 220f, 0.6f, ToneShape.Triangle, 0.6f, sweepDown: true);
        private void OnRewardedAdCompleted(RewardedAdCompletedEvent e) => Play("reward_collect", 700f, 0.3f, ToneShape.Sine, 0.45f);

        private void OnUnitDied(UnitDiedEvent e)
        {
            if (e.Team == TeamSide.Enemy) Play("enemy_death", e.IsStructure ? 130f : 260f, 0.25f, ToneShape.Square, e.IsStructure ? 0.7f : 0.35f, sweepDown: true);
            else Play("friendly_death", 180f, 0.2f, ToneShape.Triangle, 0.3f, sweepDown: true);
        }

        public void PlayButtonClick() => Play("button_click", 900f, 0.06f, ToneShape.Square, 0.3f);

        private void Play(string key, float freqHz, float duration, ToneShape shape, float volume, bool sweepDown = false)
        {
            if (!_clipCache.TryGetValue(key, out var clip))
            {
                clip = ProceduralAudioFactory.CreateTone(key, freqHz, duration, shape, volume, sweepDown);
                _clipCache[key] = clip;
            }

            var src = _sfxPool[_sfxCursor];
            _sfxCursor = (_sfxCursor + 1) % _sfxPool.Count;
            src.PlayOneShot(clip, AudioSettingsStore.SfxVolume);
        }

        public void ApplySettings()
        {
            _musicSourceA.volume = AudioSettingsStore.MusicVolume;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || _musicSourceA.clip == clip) return;
            _musicSourceA.clip = clip;
            _musicSourceA.Play();
        }

        /// <summary>0 = calm, 1 = intense. With real clips assigned this crossfades layers;
        /// with none assigned it is a safe no-op.</summary>
        public void SetMusicIntensity(float intensity01)
        {
            _musicSourceB.volume = Mathf.Clamp01(intensity01) * AudioSettingsStore.MusicVolume;
            _musicSourceA.volume = (1f - Mathf.Clamp01(intensity01)) * AudioSettingsStore.MusicVolume;
        }

        public static void TriggerHaptic()
        {
            if (!AudioSettingsStore.HapticsEnabled) return;
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
