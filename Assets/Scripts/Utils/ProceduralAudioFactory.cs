using UnityEngine;

namespace CrashMissileCrash.Utils
{
    public enum ToneShape { Sine, Square, Triangle, Noise }

    /// <summary>
    /// Synthesizes short SFX AudioClips at runtime (simple envelope over a sine/square/triangle/
    /// noise waveform) so the game has real, distinct audio feedback for every action without
    /// requiring authored sound assets. Swap any generated clip for a sound-designed one later by
    /// assigning it in AudioManager - gameplay code only ever asks for a sound by key.
    /// </summary>
    public static class ProceduralAudioFactory
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateTone(string name, float frequencyHz, float durationSeconds, ToneShape shape, float volume = 0.6f, bool pitchSweepDown = false)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * durationSeconds));
            var data = new float[sampleCount];
            var rng = new System.Random(Mathf.RoundToInt(frequencyHz * 1000));

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;
                float progress = i / (float)sampleCount;
                float freq = pitchSweepDown ? Mathf.Lerp(frequencyHz * 1.6f, frequencyHz * 0.6f, progress) : frequencyHz;
                float phase = 2f * Mathf.PI * freq * t;

                float sample = shape switch
                {
                    ToneShape.Sine => Mathf.Sin(phase),
                    ToneShape.Square => Mathf.Sign(Mathf.Sin(phase)),
                    ToneShape.Triangle => Mathf.PingPong(phase / Mathf.PI, 1f) * 2f - 1f,
                    ToneShape.Noise => (float)(rng.NextDouble() * 2.0 - 1.0),
                    _ => 0f
                };

                float envelope = Mathf.Min(progress * 12f, 1f) * Mathf.Min((1f - progress) * 6f, 1f);
                data[i] = sample * envelope * volume;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
